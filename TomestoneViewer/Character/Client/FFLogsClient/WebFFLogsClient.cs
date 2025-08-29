using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

    public async Task Fetch(CharacterId characterId)
    {
        var signature = await FetchSignature(characterId);
        var zone = 59;
        var boss = 1073;
        if (signature != null)
        {
            var uri = $"https://www.fflogs.com/character/rankings-zone/"
                + $"{signature.Id}/dsp/3/{zone}/{boss}/5000/0/-1/Any/rankings/0/0?"
                + $"dpstype=rdps&class=Any&signature={signature.Sig}";
            uri = $"https://www.fflogs.com/character/rankings-zone/{signature.Id}/dps/3/{zone}/{boss}/5000/0/-1/Any/rankings/0/0?dpstype=rdps&class=Any&signature={signature.Sig}";
            Service.PluginLog.Info($"uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("referer", "https://www.fflogs.com/");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "_token", signature.Token } });
            var response = await this.httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var parser = new SimpleParser(content);
                int count = 0;
                while (parser.Seek("rank-percent boss-hist-cell"))
                {
                    count++;
                }
                Service.PluginLog.Info($"Fetched count {count}");
            }
            else
            {
                Service.PluginLog.Error($"Can't Fetch for {characterId}: {response.StatusCode} {content}");
            }

        }
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
