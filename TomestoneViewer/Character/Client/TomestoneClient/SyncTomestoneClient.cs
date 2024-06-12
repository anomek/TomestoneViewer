using System.Collections.Concurrent;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Encounter.Data.Tomestone;

namespace TomestoneViewer.Character.Client.TomestoneClient;

/// <summary>
/// Tricky name, methods are still async, but this client prevents calling same methods with same params more then once at the time.
/// See SyncQuery class.
/// </summary>
internal class SyncTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    private readonly ConcurrentDictionary<CharacterId, SyncQuery<ClientResponse<LodestoneId, TomestoneClientError>>> lodestoneId = [];
    private readonly ConcurrentDictionary<LodestoneId, SyncQuery<ClientResponse<CharacterSummary, TomestoneClientError>>> characterSummary = [];
    private readonly ConcurrentDictionary<(LodestoneId, Location), SyncQuery<ClientResponse<TomestoneData, TomestoneClientError>>> encounter = [];

    public async Task<ClientResponse<LodestoneId, TomestoneClientError>> FetchLodestoneId(CharacterId characterId)
    {
        return await this.lodestoneId.GetOrAdd(characterId, arg => new SyncQuery<ClientResponse<LodestoneId, TomestoneClientError>>(() => this.client.FetchLodestoneId(arg)))
            .Run();
    }

    public async Task<ClientResponse<CharacterSummary, TomestoneClientError>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await this.characterSummary.GetOrAdd(lodestoneId, arg => new SyncQuery<ClientResponse<CharacterSummary, TomestoneClientError>>(() => this.client.FetchCharacterSummary(arg)))
            .Run();
    }

    public async Task<ClientResponse<TomestoneData, TomestoneClientError>> FetchEncounter(LodestoneId lodestoneId, Location location)
    {
        return await this.encounter.GetOrAdd((lodestoneId, location), arg => new SyncQuery<ClientResponse<TomestoneData, TomestoneClientError>>(() => this.client.FetchEncounter(lodestoneId, location)))
            .Run();
    }
}
