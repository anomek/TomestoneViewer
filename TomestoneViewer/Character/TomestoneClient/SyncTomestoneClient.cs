using System.Collections.Concurrent;
using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

/// <summary>
/// Tricky name, methods are still async, but this client prevents calling same methods with same params more then once at the time
/// See SyncQuery class
/// </summary>
internal class SyncTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    private readonly ConcurrentDictionary<CharacterId, SyncQuery<ClientResponse<LodestoneId>>> lodestoneId = [];
    private readonly ConcurrentDictionary<LodestoneId, SyncQuery<ClientResponse<CharacterSummary>>> characterSummary = [];
    // TODO: change string type when refactoring encounter
    private readonly ConcurrentDictionary<(LodestoneId, string), SyncQuery<ClientResponse<EncounterProgress>>> encounter = [];


    public async Task<ClientResponse<LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return await this.lodestoneId.GetOrAdd(characterId, arg => new SyncQuery<ClientResponse<LodestoneId>>(() => this.client.FetchLodestoneId(arg)))
            .Run();
    }

    public async Task<ClientResponse<CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await this.characterSummary.GetOrAdd(lodestoneId, arg => new SyncQuery<ClientResponse<CharacterSummary>>(() => this.client.FetchCharacterSummary(arg)))
            .Run();
    }

    public async Task<ClientResponse<EncounterProgress>> FetchEncounter(LodestoneId lodestoneId, EncounterLocation.Category category, EncounterLocation location)
    {
        return await this.encounter.GetOrAdd((lodestoneId, location.DisplayName), arg => new SyncQuery<ClientResponse<EncounterProgress>>(() => this.client.FetchEncounter(lodestoneId, category, location)))
            .Run();
    }
}
