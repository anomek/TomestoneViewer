using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

internal class CachedTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    private readonly Cache<CharacterId, LodestoneId, TomestoneClientError> lodestoneIdCache = new();
    private readonly Cache<LodestoneId, CharacterSummary, TomestoneClientError> characterSummaryCache = new();
    private readonly Cache<(LodestoneId, string), TomestoneEncounterData, TomestoneClientError> encounterProgressCache = new();

    public async Task<ClientResponse<TomestoneClientError, LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return await this.lodestoneIdCache.Get(characterId, () => this.client.FetchLodestoneId(characterId));
    }

    public async Task<ClientResponse<TomestoneClientError, CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await this.characterSummaryCache.Get(lodestoneId, () => this.client.FetchCharacterSummary(lodestoneId));
    }

    public async Task<ClientResponse<TomestoneClientError, TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, TomestoneLocation location)
    {
        return await this.encounterProgressCache.Get((lodestoneId, location.EncounterQueryParam), () => this.client.FetchEncounter(lodestoneId, location));
    }
}
