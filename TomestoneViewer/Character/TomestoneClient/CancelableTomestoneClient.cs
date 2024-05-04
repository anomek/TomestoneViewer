using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public class CancelableTomestoneClient(ITomestoneClient clinet) : ITomestoneClient
{
    private readonly ITomestoneClient client = clinet;
    private bool canceled;

    public void Cancel()
    {
        this.canceled = true;
    }

    public async Task<ClientResponse<CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
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

    public async Task<ClientResponse<EncounterProgress>> FetchEncounter(LodestoneId lodestoneId, EncounterLocation.Category category, EncounterLocation location)
    {
        if (this.canceled)
        {
            return new(TomestoneClientError.RequestCancelled);
        }
        else
        {
            return await this.client.FetchEncounter(lodestoneId, category, location);
        }
    }

    public async Task<ClientResponse<LodestoneId>> FetchLodestoneId(CharacterId characterId)
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
