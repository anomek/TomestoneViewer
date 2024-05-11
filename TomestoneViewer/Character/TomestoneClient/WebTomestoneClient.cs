using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

internal partial class WebTomestoneClient : ITomestoneClient
{
    private readonly LowLevelTomestoneClient client = new();

    public async Task<ClientResponse<CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        Service.PluginLog.Info($"WebTomestoneClient.FetchCharacterSummary {lodestoneId}");
        return (await this.client.GetDynamic($"https://tomestone.gg/character/{lodestoneId}/dummy", "headerEncounters", TomestoneClientError.CharacterTomestoneDisabled))
            .FlatMap(this.ParseCharacterSummary);
    }

    public async Task<ClientResponse<EncounterProgress>> FetchEncounter(LodestoneId lodestoneId, Location location)
    {
        Service.PluginLog.Info($"WebTomestoneClient.FetchEncounter {lodestoneId} {location.DisplayName}");
        var url = $"https://tomestone.gg/character/{lodestoneId}/dummy/activity?"
            + $"expansion={location.ExpansionQueryParam}&category={location.Category.CategoryQueryParam}&zone={location.Category.ZoneQueryParam}"
            + $"&encounter={location.EncounterQueryParam}&sortType=firstKillTime";

        return (await this.client.GetDynamic(url, "characterPageContent", TomestoneClientError.CharacterActivityStreamDisabled))
            .FlatMap(this.ParseEncounter);
    }

    public async Task<ClientResponse<LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        Service.PluginLog.Info($"WebTomestoneClient.FetchLodestoneId {characterId}");
        return (await this.client.GetDirect($"https://tomestone.gg/character-name/{characterId.World}/{characterId.FullName}"))
            .FlatMap(Dispose(this.ParseLodestoneIdResponse));
    }

    private ClientResponse<CharacterSummary> ParseCharacterSummary(dynamic? response)
    {
        if (response == null)
        {
            Service.PluginLog.Error($"Failed to parse character summary: response is null");
            return new(TomestoneClientError.ServerResponseError);
        }

        var ultimates = response?.props.headerEncounters?.latestExpansion?.ultimate;
        Dictionary<UltimateId, EncounterClear> summary = [];
        if (ultimates != null)
        {
            foreach (var ultimate in ultimates)
            {
                var id = new UltimateId((int)ultimate.id);
                if (ultimate.achievement != null)
                {
                    summary[id] = new EncounterClear(
                        ParseDateTime(ultimate.achievement.completedAt),
                        ultimate.achievement.completionWeek.ToString());
                }
                else if (ultimate.activity != null && (bool)ultimate.activity)
                {
                    summary[id] = new EncounterClear(null, null);
                }
            }
        }

        return new(new CharacterSummary(summary));
    }

    private ClientResponse<EncounterProgress> ParseEncounter(dynamic? response)
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

        var data = activities.activities?.paginator?.data;
        if (data == null)
        {
            Service.PluginLog.Error($"Failed to parse encounter: data is null");
            return new(TomestoneClientError.ServerResponseError);
        }

        List<ProgPoint.Lockout> lockouts = [];
        foreach (var activity in data)
        {
            if (activity.killsCount > 0)
            {
                return new(EncounterProgress.EncounterCleared(new EncounterClear(
                    ParseDateTime(activity.endTime),
                    null)));
            }
            else
            {
                var lockout = new ProgPoint.Lockout(
                    ProgPoint.Percent.From(activity.bestPercent.ToString()),
                    DateOnly.FromDateTime(ParseDateTime(activity.endTime)));
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
            return new(EncounterProgress.EncounterInProgress(new ProgPoint(lockouts)));
        }
        else
        {
            return new(EncounterProgress.EncounterNotStarted());
        }
    }

    private ClientResponse<LodestoneId> ParseLodestoneIdResponse(HttpResponseMessage result)
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

    private static DateTime ParseDateTime(object value)
    {
        return DateTime.ParseExact(value?.ToString() ?? string.Empty, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    [GeneratedRegex("https://tomestone.gg/character/([0-9]+)/")]
    private static partial Regex LocationRegex();
}
