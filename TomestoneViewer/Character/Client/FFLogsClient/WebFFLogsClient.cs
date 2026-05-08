using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class WebFFLogsClient : IFFLogsClient
{
    private readonly LowLevelFFLogsClient client;

    internal WebFFLogsClient(LowLevelFFLogsClient client)
    {
        this.client = client;
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(LodestoneId character, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        Service.PluginLog.Info($"Fetching fflogs encounter for {character} on {location}");
        var uri = $"https://www.fflogs.com/api/v2/client";

        HttpRequestMessage Request()
        {
            var r = new HttpRequestMessage(HttpMethod.Post, uri);
            var query = $"query CharacterData {{ characterData  {{ character(lodestoneID: {character}) {{ encounterRankings(encounterID:{location.BossId}) }} }} }}";
            var payload = new Dictionary<string, object>();
            payload["query"] = query;
            payload["variables"] = new object();
            r.Content = new StringContent(JsonConvert.SerializeObject(payload), new MediaTypeHeaderValue("application/json"));
            return r;
        }

        return (await this.client.Call(Request, cancellationToken))
            .Map<FFLogsEncounterData>(content => this.ParseFetchEncounter(content, location));
    }

    private FFLogsEncounterData ParseFetchEncounter(dynamic? content, FFLogsLocation.FFLogsZone location)
    {
        Dictionary<JobId, uint> clearsCount = [];
        Dictionary<JobId, DateTimeOffset> lastClear = [];

        var encounterRankings = content?.data?.characterData?.character?.encounterRankings as JObject;
        var encounters = encounterRankings?["ranks"] as JArray;
        if (encounters != null)
        {
            foreach (dynamic ranking in encounters)
            {
                var datetime = DateTimeOffset.FromUnixTimeMilliseconds((long)ranking.startTime + (long)ranking.duration);
                var job = JobId.FromFFLogsString((string)ranking.spec);
                if (clearsCount.ContainsKey(job))
                {
                    clearsCount[job]++;
                }
                else
                {
                    clearsCount[job] = 1;
                }

                if (!lastClear.TryGetValue(job, out DateTimeOffset value) || value < datetime)
                {
                    lastClear[job] = datetime;
                }
            }
        }

        return new FFLogsEncounterData(
            clearsCount.Keys
            .Select(jobid => (jobid, new FFLogsEncounterData.CClearCount(clearsCount[jobid], lastClear[jobid], location.PreviousExpansion)))
            .ToDictionary());
    }
}
