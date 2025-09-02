using FFXIVClientStructs.FFXIV.Client.System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class LowLevelFFLogsClient
{
    private static readonly TimeSpan COOLDOWN = TimeSpan.FromSeconds(70);
    private static readonly TimeSpan MINIMUM_DELAY = TimeSpan.FromSeconds(20);
    private static readonly int LIMIT = 30;

    private readonly HttpClient httpClient;
    private readonly SemaphoreSlim semaphore = new(1, 1);

    private Queue<DateTime> successRequests = new();

    internal LowLevelFFLogsClient()
    {
        var handler = new HttpClientHandler()
        {
            UseCookies = true,
            AllowAutoRedirect = false,
        };

        this.httpClient = new HttpClient(handler);
        this.httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
    }



    internal async Task<ClientResponse<FFLogsClientError, string>> Call(Func<HttpRequestMessage> request, CancellationToken token)
    {
        await this.semaphore.WaitAsync();
        try
        {
            TimeSpan? delay;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return new(FFLogsClientError.RequestCancelled);
                }

                delay = this.WaitTime();
                if (delay != null)
                {
                    Service.PluginLog.Info($"Waiting until limit refreshes {delay}");
                    await Task.Delay(delay.Value);
                    continue;
                }

                if (token.IsCancellationRequested)
                {
                    return new(FFLogsClientError.RequestCancelled);
                }

                var response = await this.httpClient.SendAsync(request.Invoke());
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Service.PluginLog.Info($"FFLogsClient: Too many requests");
                    await Task.Delay(this.WaitTime().GetValueOrDefault(MINIMUM_DELAY));
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    this.successRequests.Enqueue(DateTime.Now);
                    return new(content);
                }
                else if (response.StatusCode == (HttpStatusCode)419)
                {
                    Service.PluginLog.Error($"FFLogs returned content expired");
                    return new(FFLogsClientError.ContentExpired);
                }
                else
                {
                    Service.PluginLog.Error($"FFLogs call failed: {response.StatusCode} {content}");
                    return new(FFLogsClientError.ServerResponseError);
                }


                //// Handle 419 to refresh page to grab new token
                //if (response.StatusCode != HttpStatusCode.OK)
            }
        }
        finally
        {
            this.semaphore.Release();
        }
    }


    private TimeSpan? WaitTime()
    {
        DateTime now = DateTime.Now;
        while (this.successRequests.TryPeek(out var result) && (now - result) > COOLDOWN)
        {
            this.successRequests.Dequeue();
        }

        if (this.successRequests.Count < LIMIT)
        {
            return null;
        }

        return COOLDOWN - (now - this.successRequests.ElementAt(LIMIT - 1));
    }
}
