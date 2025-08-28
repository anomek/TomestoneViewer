using System.Collections.Concurrent;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

/// <summary>
/// Tricky name, methods are still async, but this client prevents calling same methods with same params more then once at the time.
/// See SyncQuery class.
/// </summary>
internal class SyncTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    private readonly ConcurrentDictionary<CharacterId, SyncQuery<ClientResponse<LodestoneId>>> lodestoneId = [];
    private readonly ConcurrentDictionary<LodestoneId, SyncQuery<ClientResponse<CharacterSummary>>> characterSummary = [];
    private readonly ConcurrentDictionary<(LodestoneId, Location), SyncQuery<ClientResponse<TomestoneEncounterData>>> encounter = [];

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

    public async Task<ClientResponse<TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, Location location)
    {
        return await this.encounter.GetOrAdd((lodestoneId, location), arg => new SyncQuery<ClientResponse<TomestoneEncounterData>>(() => this.client.FetchEncounter(lodestoneId, location)))
            .Run();
    }
}
