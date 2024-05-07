using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TomestoneViewer.Character.TomestoneClient;

internal partial class LowLevelTomestoneClient
{
    private const int RetryCount = 3;

    private readonly HttpClient httpClient;
    private readonly SyncQuery<ClientResponse<string>> getInertiaVersionQuery;

    private string? inertiaVersion;

    internal LowLevelTomestoneClient()
    {
        var handler = new HttpClientHandler()
        {
            UseCookies = false,
            AllowAutoRedirect = false,
        };
        this.httpClient = new HttpClient(handler);
        this.getInertiaVersionQuery = new(this.FetchInertiaVersion);
    }

    internal async Task<ClientResponse<HttpResponseMessage>> GetDirect(string uri)
    {
        try
        {
            return new(await this.httpClient.GetAsync(uri));
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, $"Direct request to {uri} failed");
            return new(TomestoneClientError.NetworkError);
        }
    }

    internal async Task<ClientResponse<dynamic?>> GetDynamic(string uri, string partialData, TomestoneClientError notFoundError)
    {
        TomestoneClientError? lastError = null;
        for (var i = 0; i < RetryCount; i++)
        {
            if (this.inertiaVersion == null)
            {
                var fetchInertiaError = await this.RefreshIntertia(this.inertiaVersion);
                if (fetchInertiaError != null)
                {
                    lastError = fetchInertiaError;
                    continue;
                }
            }

            HttpResponseMessage? response = null;
            try
            {
                var localInertiaVersion = this.inertiaVersion;
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("accept", "text/html, application/xhtml+xml");
                request.Headers.Add("accept-language", "en-US,en;q=0.9");
                request.Headers.Add("x-inertia", "true");
                request.Headers.Add("x-inertia-version", localInertiaVersion);
                request.Headers.Add("x-inertia-partial-component", "Characters/Character");
                request.Headers.Add("x-inertia-partial-data", partialData);
                response = await this.httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var fetchInertiaError = await this.RefreshIntertia(localInertiaVersion);
                    if (fetchInertiaError != null)
                    {
                        lastError = fetchInertiaError;
                    }

                    continue;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new(notFoundError);
                }
                else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Service.PluginLog.Info($"Dynamic request to {uri} failed with {response.StatusCode}: {jsonContent}");
                    lastError = TomestoneClientError.ServerResponseError;
                    continue;
                }

                return new(JsonConvert.DeserializeObject(jsonContent));
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, $"Dynamic requets to {uri} failed.");
                lastError = TomestoneClientError.NetworkError;
            }
            finally
            {
                response?.Dispose();
            }
        }

        return new(lastError ?? TomestoneClientError.NetworkError);
    }

    private async Task<TomestoneClientError?> RefreshIntertia(string? invalidInertia)
    {
        if (this.inertiaVersion != null && !string.Equals(invalidInertia, this.inertiaVersion))
        {
            // we already have newer inertia, no need to refresh it
            return null;
        }

        return (await this.getInertiaVersionQuery.Run())
            .Fold<TomestoneClientError?>(
            inertiaVersion =>
            {
                this.inertiaVersion = inertiaVersion;
                return null;
            },
            error => error);
    }

    private async Task<ClientResponse<string>> FetchInertiaVersion()
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await this.httpClient.GetAsync("https://tomestone.gg/");
            var content = await response.Content.ReadAsStringAsync();
            var match = InertiaRegex().Match(content);
            if (match.Success)
            {
                var version = match.Groups[1].Value;
                return new(version);
            }
            else
            {
                Service.PluginLog.Error($"Can't find version in response: {content}");
                return new(TomestoneClientError.InertiaVersionNotFound);
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, $"Request to fetch inertia failed");
            return new(TomestoneClientError.NetworkError);
        }
        finally
        {
            response?.Dispose();
        }
    }

    [GeneratedRegex("version&quot;:&quot;([a-z0-9]+)&quot;}\"")]
    private static partial Regex InertiaRegex();
}
