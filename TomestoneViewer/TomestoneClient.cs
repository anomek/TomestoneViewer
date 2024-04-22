using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TomestoneViewer.Model;
using Newtonsoft.Json;

namespace TomestoneViewer;

public class TomestoneClient
{
    private readonly Regex inertiaRegex = new Regex("version&quot;:&quot;([a-z0-9]+)&quot;}\"");
    private readonly Regex locationRegex = new Regex("https://tomestone.gg/character/([0-9]+)/");
    private readonly HttpClient httpClient;

    private string? inertiaVersion;

    private Task? inertiaTask;

    private Dictionary<CharacterId, uint?> lodestoneIdCache = [];
    private Dictionary<uint, List<int>> summaryCache = [];
    private Dictionary<(uint, string), EncounterResponse> encounterCache = [];

    public TomestoneClient()
    {
        var handler = new HttpClientHandler()
        {
            UseCookies = false,
            AllowAutoRedirect = false,
        };
        this.httpClient = new HttpClient(handler);
    }

    public async Task FetchLogs(CharData charData, string? encounterDisplayName)
    {
        // TODO: move it out of client
        Service.HistoryManager.AddHistoryEntry(charData);

        await this.FetchLodestoneId(charData);
        if (charData.LodestoneId == null)
        {
            charData.CharError = CharacterError.CharacterNotFound;
            return;
        }

        var destination = charData.EncounterData;
        var summary = await this.FetchSummary(charData);
        if (summary == null)
        {
            charData.CharError = CharacterError.CharacterNotFoundTomestone;
            return;
        }
        List<Task> tasks = [];

        foreach (var category in EncounterLocation.LOCATIONS)
        {
            foreach (var location in category.Locations)
            {
                if (location.Id.HasValue && summary.Contains(location.Id.Value))
                {
                    destination[location.DisplayName].LoadCleared();
                }
                else if (encounterDisplayName == null || encounterDisplayName == location.DisplayName)
                {
                    Service.PluginLog.Info($"Fetching {location.DisplayName} for {charData.CharId}");
                    tasks.Add(this.FetchPage(category, location, charData)
                        .ContinueWith(t => t.Result.Reponse.Apply(destination[t.Result.DisplayName])));
                }
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task<List<int>?> FetchSummary(CharData charData)
    {
        if (!charData.IsActive)
        {
            return [];
        }

        var lodestoneId = charData.LodestoneId.Value;
        if (this.summaryCache.TryGetValue(lodestoneId, out var data))
        {
            return data;
        }
        else
        {
            var json = await this.Fetch(charData, $"https://tomestone.gg/character/{charData.LodestoneId}/dummy", "headerEncounters");
            if ((json is System.Net.HttpStatusCode) && json == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            data = this.ParseSummary(json);
            this.summaryCache[lodestoneId] = data;
            return data;
        }
    }

    private List<int> ParseSummary(dynamic? response)
    {
        var ultimates = response?.props.headerEncounters?.latestExpansion?.ultimate;
        List<int> summary = [];
        if (ultimates != null)
        {
            foreach (var ultimate in ultimates)
            {
                if (ultimate.achievement != null || (ultimate.activity != null && (bool)ultimate.activity))
                {
                    summary.Add((int)ultimate.id);
                }
            }
        }

        return summary;
    }

    private async Task<(string DisplayName, EncounterResponse Reponse)> FetchPage(EncounterLocation.Category category, EncounterLocation location, CharData charData)
    {
        var lodestoneId = charData.LodestoneId.Value;
        if (this.encounterCache.TryGetValue((lodestoneId, location.DisplayName), out var data))
        {
            return (location.DisplayName, data);
        }
        else
        {
            var url = string.Format(
                "https://tomestone.gg/character/{0}/dummy/activity?expansion={1}&category={2}&zone={3}&encounter={4}&sortType=firstKillTime",
                charData.LodestoneId,
                location.ExpansionQueryParam,
                category.CategoryQueryParam,
                category.ZoneQueryParam,
                location.EncounterQueryParam);
            data = this.ParseEncounter(await this.Fetch(charData, url, "characterPageContent"));
            if (!data.HasError())
            {
                this.encounterCache[(lodestoneId, location.DisplayName)] = data;
            }

            return (location.DisplayName, data);
        }
    }

    private EncounterResponse ParseEncounter(dynamic? response)
    {
        var data = response?.props.characterPageContent.activities.activities.activities.paginator.data;
        if (data == null)
        {
            return new EncounterResponse()
            {
                error = CharacterError.NetworkError,
            };
        }
        else
        {
            foreach (var activity in data)
            {
                return new EncounterResponse()
                {
                    bestPercent = activity.bestPercent,
                    cleared = activity.killsCount > 0,
                };
            }

            return new EncounterResponse();
        }
    }

    private async Task<dynamic?> Fetch(CharData charData, string url, string partialData)
    {
        for (int i = 0; i < 3; i++)
        {
            if (!charData.IsActive)
            {
                return null;
            }

            if (this.inertiaVersion == null)
            {
                await this.RefreshIntertia(this.inertiaVersion);
            }

            try
            {
                var localInertiaVersion = this.inertiaVersion;
                Service.PluginLog.Info($"fetching: {url}");

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("accept", "text/html, application/xhtml+xml");
                request.Headers.Add("accept-language", "en-US,en;q=0.9");
                request.Headers.Add("x-inertia", "true");
                request.Headers.Add("x-inertia-version", localInertiaVersion);
                request.Headers.Add("x-inertia-partial-component", "Characters/Character");
                request.Headers.Add("x-inertia-partial-data", partialData);
                var response = await this.httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    await this.RefreshIntertia(localInertiaVersion);
                    continue;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return System.Net.HttpStatusCode.NotFound;
                }
                else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Service.PluginLog.Info($"Got response {response.StatusCode}: {jsonContent}");
                }

                return JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, "Error while fetching data.");
                return null;
            }
        }

        return null;
    }


    private async Task FetchLodestoneId(CharData charData)
    {
        if (charData.LodestoneId != null)
        {
            return;
        }

        if (this.lodestoneIdCache.TryGetValue(charData.CharId, out var lodestoneId))
        {
            charData.LodestoneId = lodestoneId;
            return;
        }

        try
        {
            var result = await this.httpClient.GetAsync($"https://tomestone.gg/character-name/{charData.CharId.World}/{charData.CharId.FullName}");
            if (result.StatusCode == System.Net.HttpStatusCode.Found)
            {
                var location = result.Headers.Location.ToString();
                var match = this.locationRegex.Match(location);
                if (match.Success)
                {

                    charData.LodestoneId = uint.Parse(match.Groups[1].Value);
                    this.lodestoneIdCache[charData.CharId] = charData.LodestoneId.Value;
                }
                else
                {
                    Service.PluginLog.Info($"Can't find lodestoneId in {location}");
                }
            }
            else if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                this.lodestoneIdCache[charData.CharId] = null;
                charData.CharError = CharacterError.CharacterNotFound;
            }
            else
            {
                Service.PluginLog.Info($"Request to find lodestoneId returned {result.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error while fetching LodestoneId.");
        }
    }

    private async Task RefreshIntertia(string? invalidInertia)
    {
        Service.PluginLog.Info("Requesting refresh of inetria version");
        if (this.inertiaVersion != null && !string.Equals(invalidInertia, this.inertiaVersion))
        {
            return;
        }

        if (this.inertiaTask == null)
        {
            lock (this)
            {
                this.inertiaTask ??= this.FetchInertia().ContinueWith(t => { this.inertiaVersion = t.Result; });
            }
        }

        await this.inertiaTask;
    }

    private async Task<string?> FetchInertia()
    {
        var response = await this.httpClient.GetAsync("https://tomestone.gg/");
        var content = await response.Content.ReadAsStringAsync();
        var match = this.inertiaRegex.Match(content);
        if (match.Success)
        {
            var version = match.Groups[1].Value;
            Service.PluginLog.Info($"Found inertiaVersion: {version}");
            return version;
        }
        else
        {
            Service.PluginLog.Warning($"Can't find version in response: {content}");
            return null;
        }
    }

    public class EncounterResponse
    {
        public string? bestPercent = null;
        public bool cleared = false;
        public CharacterError? error = null;

        public void Apply(EncounterData encounterData)
        {
            if (this.error.HasValue)
            {
                encounterData.LoadError(this.error.Value);
            }
            else if (this.cleared)
            {
                encounterData.LoadCleared();
            }
            else
            {
                encounterData.LoadBestPercent(this.bestPercent);
            }
        }

        public bool HasError()
        {
            return error.HasValue;
        }
    }
}
