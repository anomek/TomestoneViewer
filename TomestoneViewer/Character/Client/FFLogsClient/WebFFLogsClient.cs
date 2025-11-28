using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Client;
using TomestoneViewer.Character.Client.TomestoneClient;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class WebFFLogsClient : IFFLogsClient
{
    private readonly LowLevelFFLogsClient client = new();

    internal WebFFLogsClient()
    {
    }


    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(LodestoneId character, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        Service.PluginLog.Info($"Fetching fflogs encounter for {character} on {location}");
        var uri = $"https://www.fflogs.com/api/v2/client";
        
        var request = () =>
        {
            var r = new HttpRequestMessage(HttpMethod.Post, uri);
            var query = $"query CharacterData {{ characterData  {{ character(lodestoneID: {character}) {{ encounterRankings(encounterID:{location.BossId}) }} }} }}";
            var payload = new Dictionary<string, object>();
            payload["query"] = query;
            payload["variables"] = new object();
            r.Content = new StringContent(JsonConvert.SerializeObject(payload), new MediaTypeHeaderValue("application/json"));
            Service.PluginLog.Info($"{r.Content}");
            return r;
        };
        return (await this.client.Call(request, cancellationToken))
            .Map<FFLogsEncounterData>(content => this.ParseFetchEncounter(content, location));
    }

    private FFLogsEncounterData ParseFetchEncounter(dynamic? content, FFLogsLocation.FFLogsZone location)
    {
        Dictionary<JobId, uint> clearsCount = [];
        Dictionary<JobId, DateTimeOffset> lastClear = [];

        var encounters = content?.data?.characterData?.character?.encounterRankings?.ranks;
        if (encounters != null)
        {
            foreach (var ranking in encounters)
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
