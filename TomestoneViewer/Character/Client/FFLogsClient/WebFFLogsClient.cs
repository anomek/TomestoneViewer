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

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class WebFFLogsClient : IFFLogsClient
{
    private readonly LowLevelFFLogsClient client = new();
    private readonly SyncQuery<ClientResponse<FFLogsClientError, string>> queryToken;

    private string token = string.Empty;

    internal WebFFLogsClient()
    {
        this.queryToken = new(() => this.UpdateToken());
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsCharId>> FetchCharacter(CharacterId characterId, CancellationToken cancellationToken)
    {
        Service.PluginLog.Info($"Fetching fflogs character {characterId}");
        var region = "na"; // TODO: get region from world
        var request = () => new HttpRequestMessage(HttpMethod.Get, $"https://www.fflogs.com/character/{region}/{characterId.World.ToLower()}/{characterId.FirstName.ToLower()}%20{characterId.LastName.ToLower()}");
        return (await this.client.Call(request, cancellationToken))
            .FlatMap(this.ParseFetchCharacter);
    }


  

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(FFLogsCharId character, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        Service.PluginLog.Info($"Fetching fflogs encounter for {character} on {location}");
        var uri = $"https://www.fflogs.com/character/rankings-zone/{character.Id}/dps/3/{location.ZoneId}/{location.BossId}/5000/0/-1/Any/rankings/0/0?dpstype=rdps&class=Any&signature={character.Sig}";
        var request = () =>
        {
            var r = new HttpRequestMessage(HttpMethod.Post, uri);
            r.Headers.Add("referer", "https://www.fflogs.com/");
            r.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "_token", this.token } });
            return r;
        };
        return await this.client.Call(request, cancellationToken)
            .RecoverAsync(error => error == FFLogsClientError.ContentExpired, () =>
            {
               return this.UpdateToken()
                    .FlatMapAsync(ignored => this.client.Call(request, cancellationToken));
            })
            .Map(content => this.ParseFetchEncounter(content, location));
    }

    private async Task<ClientResponse<FFLogsClientError, string>> UpdateToken()
    {
        var request = () => new HttpRequestMessage(HttpMethod.Get, $"https://www.fflogs.com/");

        return (await this.client.Call(request, CancellationToken.None))
            .FlatMap<string>(content =>
            {
                var parser = new SimpleParser(content);
                var token = parser.Find("<meta name=\"csrf-token\" content=\"", "\"");
                if (token != null)
                {
                    this.token = token;
                    return new(token);
                }
                else
                {
                    return new(FFLogsClientError.ContentExpired);
                }
            });
    }

    private FFLogsEncounterData ParseFetchEncounter(string content, FFLogsLocation.FFLogsZone location)
    {
        var parser = new SimpleParser(content);
        uint count = 0;
        while (parser.Seek("rank-percent boss-hist-cell"))
        {
            count++;
        }

        return new FFLogsEncounterData(new Dictionary<JobId, FFLogsEncounterData.CClearCount>() { {
                JobId.Empty, new(location.PreviousExpansion ? 0 : count, location.PreviousExpansion ? count : 0, DateOnly.MinValue)
            } });
    }

    private ClientResponse<FFLogsClientError, FFLogsCharId> ParseFetchCharacter(string content)
    {
        var parser = new SimpleParser(content);
        this.token = parser.Find("<meta name=\"csrf-token\" content=\"", "\"");
        var fflogsCharId = parser.Find("var characterID = ", ";");
        parser.Seek("'/character/rankings-zone/'");
        var signature = parser.Find("'&signature=' + '", "'");

        if (fflogsCharId != null && signature != null)
        {
            return new(new FFLogsCharId(fflogsCharId, signature));
        }
        else
        {
            Service.PluginLog.Error($"Cant find signature");
            return new(FFLogsClientError.SignatureNotFound);
        }
    }


}
