using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class CancelableFFLogsClient(IFFLogsClient client) : IFFLogsClient
{
    private readonly IFFLogsClient client = client;
    private bool canceled;

    public void Cancel()
    {
        this.canceled = true;
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(CharacterId characterId, FFLogsLocation location)
    {
        return await this.client.FetchEncounter(characterId, location);
    }
}
