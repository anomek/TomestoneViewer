using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

internal class SafeTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly SafeClientHandler<TomestoneClientError> handler = new(TomestoneClientError.InternalError);
    private readonly ITomestoneClient client = client;

    public async Task<ClientResponse<TomestoneClientError, CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await this.client.FetchCharacterSummary(lodestoneId)
            .ContinueWith(this.handler.HandleTaskErrors);
    }

    public async Task<ClientResponse<TomestoneClientError, TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, TomestoneLocation location)
    {
        return await this.client.FetchEncounter(lodestoneId, location)
            .ContinueWith(this.handler.HandleTaskErrors);
    }

    public async Task<ClientResponse<TomestoneClientError, LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return await this.client.FetchLodestoneId(characterId)
            .ContinueWith(this.handler.HandleTaskErrors);
    }
}
