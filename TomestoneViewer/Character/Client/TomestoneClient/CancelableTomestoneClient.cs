using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Encounter.Data.Tomestone;

namespace TomestoneViewer.Character.Client.TomestoneClient;

public class CancelableTomestoneClient(ITomestoneClient clinet) : ITomestoneClient
{
    private readonly ITomestoneClient client = clinet;
    private bool canceled;

    public void Cancel()
    {
        this.canceled = true;
    }

    public async Task<ClientResponse<CharacterSummary, TomestoneClientError>> FetchCharacterSummary(LodestoneId lodestoneId)
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

    public async Task<ClientResponse<TomestoneData, TomestoneClientError>> FetchEncounter(LodestoneId lodestoneId, Location location)
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

    public async Task<ClientResponse<LodestoneId, TomestoneClientError>> FetchLodestoneId(CharacterId characterId)
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
