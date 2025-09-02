using System;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class CancelableFFLogsClient(IFFLogsClient client) : IFFLogsClient
{
    private readonly IFFLogsClient client = client;
    private readonly CancellationTokenSource tokenSource = new();
    private bool canceled;

    public void Cancel()
    {
        this.canceled = true;
        this.tokenSource.Cancel();
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsCharId>> FetchCharacter(CharacterId characterId, CancellationToken cancellationToken)
    {
        if (this.canceled)
        {
            return new(FFLogsClientError.RequestCancelled);
        }
        else
        {
            return await this.GetSafe(() => this.client.FetchCharacter(characterId, this.tokenSource.Token));
        }
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(FFLogsCharId characterId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        if (this.canceled)
        {
            return new(FFLogsClientError.RequestCancelled);
        }
        else
        {
            return await this.GetSafe(() => this.client.FetchEncounter(characterId, location, this.tokenSource.Token));
        }
    }

    private async Task<ClientResponse<FFLogsClientError, T>> GetSafe<T>(Func<Task<ClientResponse<FFLogsClientError, T>>> call)
    {
        ClientResponse<FFLogsClientError, T> response;
        do
        {
            response = await call.Invoke();
        }
        while (response.HasError(error => error == FFLogsClientError.RequestCancelled) && !this.canceled);
        return response;
    }
}
