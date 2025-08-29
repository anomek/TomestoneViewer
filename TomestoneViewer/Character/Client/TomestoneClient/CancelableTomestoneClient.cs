using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

public class CancelableTomestoneClient(ITomestoneClient clinet) : ITomestoneClient
{
    private readonly ITomestoneClient client = clinet;
    private bool canceled;

    public void Cancel()
    {
        this.canceled = true;
    }

    public async Task<ClientResponse<TomestoneClientError, CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        if (this.canceled)
        {
            return new(TomestoneClientError.RequestCancelled);
        }
        else
        {
            return await this.client.FetchCharacterSummary(lodestoneId);
        }
    }

    public async Task<ClientResponse<TomestoneClientError, TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, TomestoneLocation location)
    {
        if (this.canceled)
        {
            return new(TomestoneClientError.RequestCancelled);
        }
        else
        {
            return await this.client.FetchEncounter(lodestoneId, location);
        }
    }

    public async Task<ClientResponse<TomestoneClientError, LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        if (this.canceled)
        {
            return new(TomestoneClientError.RequestCancelled);
        }
        else
        {
            return await this.client.FetchLodestoneId(characterId);
        }
    }
}
