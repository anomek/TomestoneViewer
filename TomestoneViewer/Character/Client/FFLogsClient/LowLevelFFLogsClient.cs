using FFXIVClientStructs.FFXIV.Client.System.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class LowLevelFFLogsClient
{
    private readonly HttpClient httpClient = new();

    private SyncQuery<string?> tokenQuery;

    private string? accessToken;


    internal LowLevelFFLogsClient()
    {
        tokenQuery = new(this.GetAuthToken);
    }

    private async Task<string?> GetAuthToken()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.fflogs.com/oauth/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Service.Configuration.FFLogsClientId}:{Service.Configuration.FFLogsClientSecret}")));
            request.Content = new FormUrlEncodedContent([new("grant_type", "client_credentials")]);
            var response = await this.httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = (dynamic?)JsonConvert.DeserializeObject(content);
                var token = json?.access_token;
                this.accessToken = token;
                Service.PluginLog.Info($"Acq token {this.accessToken}");
                return token;
            }
            else
            {
                Service.PluginLog.Warning($"Failed to fetch access token: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error($"Error fetching access token: {ex}");
            return null;
        }
    }

    internal async Task<ClientResponse<FFLogsClientError, dynamic?>> Call(Func<HttpRequestMessage> request, CancellationToken token)
    {

        int tries = 4;
        FFLogsClientError? lastError = null;
        while (tries > 0)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    return new(FFLogsClientError.RequestCancelled);
                }

                var requestMessage = request.Invoke();
                var accessTokenToUse = this.accessToken ?? await this.tokenQuery.Run();
                if (accessTokenToUse == null)
                {
                    Service.PluginLog.Warning("No access token, request wont be send");
                    return new(FFLogsClientError.Unauthorized);
                }

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenToUse);
                var response = await this.httpClient.SendAsync(requestMessage);

                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Service.PluginLog.Info($"FFLogs gor response {content}");
                    return new(JsonConvert.DeserializeObject(content));
                }

                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await tokenQuery.Run();
                    Service.PluginLog.Info("FFLogs call got unathorized response");
                    lastError = FFLogsClientError.Unauthorized;
                }
                else
                {
                    Service.PluginLog.Error($"FFLogs call failed: {requestMessage.Method} {requestMessage.RequestUri} {response.StatusCode} {content}");
                    return new(FFLogsClientError.ServerResponseError);
                }
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error($"Error calling fflogs: {ex}");
                lastError = FFLogsClientError.InternalError;
            }

            tries--;
        }

        Service.PluginLog.Error($"retries exhauseted with error: {lastError}");
        return new(lastError ?? FFLogsClientError.InternalError);
    }
}
