using Lumina.Excel.Sheets;
using NetStone;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;
using static FFXIVClientStructs.FFXIV.Client.System.String.Utf8String.Delegates;

namespace TomestoneViewer.Character.Client.TomestoneClient;

internal partial class WebTomestoneClient : ITomestoneClient
{
    private static readonly string TOMESTONE_URL = "https://tomestone.gg";

    private readonly LowLevelTomestoneClient client = new();
    private readonly SyncValue<LodestoneClient> lodestoneClient = new(() => LodestoneClient.GetClientAsync());

    internal WebTomestoneClient()
    {
        var ignore = this.lodestoneClient.Get();
    }

    public async Task<ClientResponse<TomestoneClientError, CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        Service.PluginLog.Info($"WebTomestoneClient.FetchCharacterSummary {lodestoneId}");
        return (await this.client.GetDynamic($"{TOMESTONE_URL}/character/{lodestoneId}/dummy", "headerEncounters,mainCharacter", TomestoneClientError.CharacterTomestoneDisabled))
            .FlatMap(this.ParseCharacterSummary);
    }

    public async Task<ClientResponse<TomestoneClientError, TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, TomestoneLocation location)
    {
        Service.PluginLog.Info($"WebTomestoneClient.FetchEncounter {lodestoneId} {location.EncounterQueryParam}");
        var url = $"{TOMESTONE_URL}/character/{lodestoneId}/dummy/activity?"
            + $"expansion={location.ExpansionQueryParam}&category={location.Category.CategoryQueryParam}&zone={location.Category.ZoneQueryParam}"
            + $"&encounter={location.EncounterQueryParam}&sortType=firstKillTime";

        return await (await this.client.GetDynamic(url, "characterPageContent", TomestoneClientError.CharacterActivityStreamDisabled))
            .FlatMapAsync<TomestoneEncounterData>(async encounterResponse =>
            {
                var progressGraph = await this.FetchProgressGraph(encounterResponse);
                return this.ParseEncounter(encounterResponse, progressGraph);
            });
    }

    public async Task<ClientResponse<TomestoneClientError, LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        Service.PluginLog.Info($"WebTomestoneClient.FetchLodestoneId {characterId}");
        try
        {
            var response = await (await this.lodestoneClient.Get())
                .SearchCharacter(new() { CharacterName = characterId.FullName, World = characterId.World });
            if (response == null)
            {
                return new(TomestoneClientError.EmptyLodestoneResponse);
            }

            var character = response.Results
                .FirstOrDefault(entry => entry.Name == characterId.FullName);

            if (character == null)
            {
                return new(TomestoneClientError.CharacterDoesNotExist);
            }

            return new(new LodestoneId(uint.Parse(character.Id)));

        }
        catch (HttpRequestException ex)
        {
            Service.PluginLog.Error($"Exception while fetching lodestone id for character {characterId}", ex);
            return new(TomestoneClientError.LodestoneClientError);
        }
    }

    private async Task<dynamic?> FetchProgressGraph(dynamic? encounterResponse)
    {
        var url = encounterResponse?.props.characterPageContent.activities.progressionGraphUrl;
        if (url == null)
        {
            Service.PluginLog.Debug("url for graph is null, skipping");
            return null;
        }
        else
        {
            Service.PluginLog.Debug($"fetching progress graph from {url}");
            return (await (await this.client.GetDirect($"{TOMESTONE_URL}{url}"))
                .FlatMap<HttpResponseMessage>(response => response.IsSuccessStatusCode ? new(response) : new(TomestoneClientError.ServerResponseError))
                .MapAsync(async response => JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync())))
                .Fold(response => response, error => null);
        }
    }

    private ClientResponse<TomestoneClientError, CharacterSummary> ParseCharacterSummary(dynamic? response)
    {
        if (response == null)
        {
            Service.PluginLog.Error($"Failed to parse character summary: response is null");
            return new(TomestoneClientError.ServerResponseError);
        }

        var headerEncounters = response?.props.headerEncounters;
        if (headerEncounters == null)
        {
            return new(TomestoneClientError.CharacterTomestoneDisabled);
        }

        var ultimates = headerEncounters?.latestExpansion?.ultimate ?? new JArray();

        // hardcoding [2] as current tier (we should make it more explicit somewhere
        var savages = headerEncounters?.latestExpansion?.savage?[2]?.encounters ?? new JArray();

        // mutating ultimates, very dirty
        ultimates.Merge(savages);

        Dictionary<TomestoneLocationId, TomestoneEncounterData> summary = [];

        foreach (var encounter in ultimates)
        {
            var id = new TomestoneLocationId((int)encounter.id);
            if (encounter.achievement != null)
            {
                summary[id] = TomestoneEncounterData.EncounterCleared(new EncounterClear(
                    ParseDateTime(encounter.achievement.completedAt),
                    encounter.achievement.completionWeek.ToString()));
            }
            else if (encounter.activity != null)
            {
                summary[id] = TomestoneEncounterData.EncounterCleared(new EncounterClear(null, null));
            }
        }

        var allUltimateProgressionTargets = headerEncounters?.allUltimateProgressionTargets;
        if (allUltimateProgressionTargets != null)
        {
            Service.PluginLog.Info($"targets: {allUltimateProgressionTargets}");
            foreach (var ultimateContainer in allUltimateProgressionTargets)
            {
                dynamic? ultimate = null;
                foreach (var child in ultimateContainer.Children())
                {
                    ultimate = child;
                }

                if (ultimate == null)
                {
                    continue;
                }

                Service.PluginLog.Info($"ultimate: {ultimate}");
                var id = new TomestoneLocationId((int)ultimate.encounter.id);
                if (!summary.ContainsKey(id))
                {
                    Service.PluginLog.Info($"prog point: {ultimate.percent.ToString()}");

                    summary[id] = TomestoneEncounterData.EncounterInProgress(new ProgPoint(
                        [new ProgPoint.Lockout(
                        ProgPoint.Percent.From(ultimate.percent.ToString()),
                        null,
                        JobId.Empty)],
                        null));
                }
            }
        }

        var mainCharacter = response.props.mainCharacter;
        CharacterId? mainCharacterId = null;
        if (mainCharacter != null)
        {
            Service.PluginLog.Info($"Found main character {mainCharacter}");
            mainCharacterId = CharacterId.FromFullName(mainCharacter.name.ToString(), mainCharacter.serverName.ToString());
            Service.PluginLog.Info($"Found main character {mainCharacterId}");
        }


        return new(new CharacterSummary(summary, mainCharacterId));
    }

    private ClientResponse<TomestoneClientError, TomestoneEncounterData> ParseEncounter(dynamic? response, dynamic? progressGraph)
    {
        if (response == null)
        {
            Service.PluginLog.Error($"Failed to parse encounter: response is null");
            return new(TomestoneClientError.ServerResponseError);
        }

        var activities = response.props.characterPageContent.activities.activities;
        if (activities == null)
        {
            return new(TomestoneClientError.CharacterActivityStreamDisabled);
        }

        var activities2 = activities.activites;
        if (activities2 == null || !(activities2 as JValue).Contains("paginator"))
        {
            return new(TomestoneEncounterData.EncounterNotStarted());
        }

        var data = activities.activities?.paginator?.data;
        if (data == null)
        {
            Service.PluginLog.Error($"Failed to parse encounter: data is null");
            return new(TomestoneClientError.ServerResponseError);
        }

        List<ProgPoint.Lockout> lockouts = [];
        foreach (var activity in data)
        {
            if (activity.activity.killsCount > 0)
            {
                return new(TomestoneEncounterData.EncounterCleared(new EncounterClear(
                    ParseDateTime(activity.endTime),
                    null)));
            }
            else
            {
                var dateTime = ParseDateTime(activity.activity.endTime);
                var lockout = new ProgPoint.Lockout(
                    ProgPoint.Percent.From(activity.activity.bestPercent.ToString()),
                    dateTime == null ? null : DateOnly.FromDateTime(dateTime),
                    new JobId((uint)activity.activity.displayCharacterJobOrSpec.id));
                if (lockouts.Count == 0 || !lockouts[^1].Equals(lockout))
                {
                    lockouts.Add(lockout);
                }
            }

            if (lockouts.Count >= 5)
            {
                break;
            }
        }

        if (lockouts.Count > 0)
        {
            return new(TomestoneEncounterData.EncounterInProgress(new ProgPoint(lockouts, this.ParseLastMechanic(progressGraph))));
        }
        else
        {
            return new(TomestoneEncounterData.EncounterNotStarted());
        }
    }

    private string? ParseLastMechanic(dynamic? progressGraph)
    {
        var graph = progressGraph?.data.graph;
        if (graph == null)
        {
            return null;
        }

        var best = 0;
        string? lastMechanic = null;
        foreach (var point in graph)
        {
            if (point.duration > best && point.mechanic != null)
            {
                best = point.duration;
                var suffix = point.mechanic.number > 1
                    ? $" #{point.mechanic.number}"
                    : string.Empty;
                lastMechanic = $"{point.mechanic.name}{suffix}";
            }
        }

        if (lastMechanic != null)
        {
            Service.PluginLog.Debug($"Found last mechanic: {lastMechanic}");
        }

        return lastMechanic;
    }

    private ClientResponse<TomestoneClientError, LodestoneId> ParseLodestoneIdResponse(HttpResponseMessage result)
    {
        if (result.StatusCode == System.Net.HttpStatusCode.Found)
        {
            var location = result.Headers.Location?.ToString();
            if (location == null)
            {
                Service.PluginLog.Error($"Failed to fetch lodestoneId: location not returned in headers");
                return new(TomestoneClientError.ServerResponseError);
            }

            var match = LocationRegex().Match(location);
            if (match.Success)
            {
                var groupOne = match.Groups[1].Value;
                if (uint.TryParse(groupOne, out var parsed))
                {
                    return new(new LodestoneId(parsed));
                }
                else
                {
                    Service.PluginLog.Error($"Failed to fetch lodestoneId: it's not an int {groupOne}");
                    return new(TomestoneClientError.ServerResponseError);
                }
            }
            else
            {
                Service.PluginLog.Error($"Failed to fetch lodestoneId: can't find id in location {location}");
                return new(TomestoneClientError.ServerResponseError);
            }
        }
        else if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new(TomestoneClientError.CharacterDoesNotExist);
        }
        else
        {
            Service.PluginLog.Error($"Failed to fetch lodestoneId: request returned status code: {result.StatusCode}");
            return new(TomestoneClientError.ServerResponseError);
        }
    }

    private static Func<HttpResponseMessage, T> Dispose<T>(Func<HttpResponseMessage, T> original)
    {
        return message =>
        {
            try
            {
                return original.Invoke(message);
            }
            finally
            {
                message.Dispose();
            }
        };
    }

    private static DateTime? ParseDateTime(object value)
    {
        var stringValue = value?.ToString();
        if (stringValue == null)
        {
            return null;
        }

        try
        {
            // FIXME: figure out if this is local datetime or what
            return DateTime.ParseExact(stringValue, ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff"], CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            Service.PluginLog.Error($"Failed to parse {stringValue}: {ex.Message}");
            return null;
        }
    }

    [GeneratedRegex("https://tomestone.gg/character/([0-9]+)/")]
    private static partial Regex LocationRegex();
}
