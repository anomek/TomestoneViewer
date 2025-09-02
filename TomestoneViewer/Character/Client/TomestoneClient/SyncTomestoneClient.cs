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

    private readonly ConcurrentDictionary<CharacterId, SyncQuery<ClientResponse<TomestoneClientError, LodestoneId>>> lodestoneId = [];
    private readonly ConcurrentDictionary<LodestoneId, SyncQuery<ClientResponse<TomestoneClientError, CharacterSummary>>> characterSummary = [];
    private readonly ConcurrentDictionary<(LodestoneId, TomestoneLocation), SyncQuery<ClientResponse<TomestoneClientError, TomestoneEncounterData>>> encounter = [];

    public async Task<ClientResponse<TomestoneClientError, LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return await this.lodestoneId.GetOrAdd(characterId, arg => new SyncQuery<ClientResponse<TomestoneClientError, LodestoneId>>(() => this.client.FetchLodestoneId(arg)))
            .Run();
    }

    public async Task<ClientResponse<TomestoneClientError, CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await this.characterSummary.GetOrAdd(lodestoneId, arg => new SyncQuery<ClientResponse<TomestoneClientError, CharacterSummary>>(() => this.client.FetchCharacterSummary(arg)))
            .Run();
    }

    public async Task<ClientResponse<TomestoneClientError, TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, TomestoneLocation location)
    {
        return await this.encounter.GetOrAdd((lodestoneId, location), arg => new SyncQuery<ClientResponse<TomestoneClientError, TomestoneEncounterData>>(() => this.client.FetchEncounter(arg.Item1, arg.Item2)))
            .Run();
    }
}
