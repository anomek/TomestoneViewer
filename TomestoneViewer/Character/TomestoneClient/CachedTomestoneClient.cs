using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

internal class CachedTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    private readonly Cache<CharacterId, LodestoneId> lodestoneIdCache = new();
    private readonly Cache<LodestoneId, CharacterSummary> characterSummaryCache = new();
    private readonly Cache<(LodestoneId, string), EncounterProgress> encounterProgressCache = new();

    public async Task<ClientResponse<LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return await this.lodestoneIdCache.Get(characterId, () => this.client.FetchLodestoneId(characterId));
    }

    public async Task<ClientResponse<CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await this.characterSummaryCache.Get(lodestoneId, () => this.client.FetchCharacterSummary(lodestoneId));
    }

    public async Task<ClientResponse<EncounterProgress>> FetchEncounter(LodestoneId lodestoneId, Location location)
    {
        return await this.encounterProgressCache.Get((lodestoneId, location.DisplayName), () => this.client.FetchEncounter(lodestoneId, location));
    }
}
