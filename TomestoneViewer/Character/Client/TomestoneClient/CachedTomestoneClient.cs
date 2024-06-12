using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Encounter.Data.Tomestone;

namespace TomestoneViewer.Character.Client.TomestoneClient;

internal class CachedTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    private readonly Cache<CharacterId, LodestoneId, TomestoneClientError> lodestoneIdCache = new();
    private readonly Cache<LodestoneId, CharacterSummary, TomestoneClientError> characterSummaryCache = new();
    private readonly Cache<(LodestoneId, string), TomestoneData, TomestoneClientError> encounterProgressCache = new();

    public async Task<ClientResponse<LodestoneId, TomestoneClientError>> FetchLodestoneId(CharacterId characterId)
    {
        return await this.lodestoneIdCache.Get(characterId, () => this.client.FetchLodestoneId(characterId));
    }

    public async Task<ClientResponse<CharacterSummary, TomestoneClientError>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await this.characterSummaryCache.Get(lodestoneId, () => this.client.FetchCharacterSummary(lodestoneId));
    }

    public async Task<ClientResponse<TomestoneData, TomestoneClientError>> FetchEncounter(LodestoneId lodestoneId, Location location)
    {
        return await this.encounterProgressCache.Get((lodestoneId, location.DisplayName), () => this.client.FetchEncounter(lodestoneId, location));
    }
}
