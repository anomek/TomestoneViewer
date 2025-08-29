using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class WebFFLogsClient : IFFLogsClient
{

    private readonly HttpClient httpClient;

    internal WebFFLogsClient()
    {
        var handler = new HttpClientHandler()
        {
            UseCookies = true,
            AllowAutoRedirect = false,
        };

        this.httpClient = new HttpClient(handler);
        this.httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(CharacterId characterId, FFLogsLocation location)
    {
        var signature = await this.FetchSignature(characterId);
        if (signature == null)
        {
            return new(FFLogsClientError.SignatureNotFound);
        }

        var uri = $"https://www.fflogs.com/character/rankings-zone/{signature.Id}/dps/3/{location.ZoneId}/{location.BossId}/5000/0/-1/Any/rankings/0/0?dpstype=rdps&class=Any&signature={signature.Sig}";

        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("referer", "https://www.fflogs.com/");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "_token", signature.Token } });
        var response = await this.httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Service.PluginLog.Error($"Can't Fetch for {characterId}: {response.StatusCode} {content}");
            return new(FFLogsClientError.ServerResponseError);
        }

        var parser = new SimpleParser(content);
        uint count = 0;
        while (parser.Seek("rank-percent boss-hist-cell"))
        {
            count++;
        }
 
        return new(new FFLogsEncounterData(new Dictionary<JobId, FFLogsEncounterData.CClearCount>() { { JobId.Empty, new(count, 0, DateOnly.MinValue) } }));
    }

    private async Task<FFLogsCharacterId?> FetchSignature(CharacterId characterId)
    {
        var region = "na"; // TODO: get region from world
        var response = await this.httpClient.GetAsync($"https://www.fflogs.com/character/{region}/{characterId.World.ToLower()}/{characterId.FirstName.ToLower()}%20{characterId.LastName.ToLower()}");
        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var parser = new SimpleParser(content);
            var token = parser.Find("<meta name=\"csrf-token\" content=\"", "\"");
            var fflogsCharId = parser.Find("var characterID = ", ";");
            parser.Seek("'/character/rankings-zone/'");
            var signature = parser.Find("'&signature=' + '", "'");

            if (fflogsCharId != null && signature != null && token != null)
            {
                return new(fflogsCharId, signature, token);
            }
            else
            {
                Service.PluginLog.Error($"Cant find signature for {characterId}");
                return null;
            }
        }
        else
        {
            Service.PluginLog.Error($"Can't fetch signature for {characterId}: {content}");
            return null;
        }
    }
}
