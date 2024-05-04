using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

// TODO: add ttl
internal class CachedTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    private readonly ConcurrentDictionary<CharacterId, ClientResponse<LodestoneId>> lodestoneIdCache = [];
    private readonly ConcurrentDictionary<LodestoneId, ClientResponse<CharacterSummary>> characterSummaryCache = [];
    private readonly ConcurrentDictionary<(LodestoneId, string), ClientResponse<EncounterProgress>> encounterProgressCache = [];

    public async Task<ClientResponse<LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return await Cache(this.lodestoneIdCache, characterId, () => this.client.FetchLodestoneId(characterId));
    }

    public async Task<ClientResponse<CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return await Cache(this.characterSummaryCache, lodestoneId, () => this.client.FetchCharacterSummary(lodestoneId));
    }

    public async Task<ClientResponse<EncounterProgress>> FetchEncounter(LodestoneId lodestoneId, EncounterLocation.Category category, EncounterLocation location)
    {
        return await Cache(this.encounterProgressCache, (lodestoneId, location.DisplayName), () => this.client.FetchEncounter(lodestoneId, category, location));
    }

    private static async Task<ClientResponse<V>> Cache<K, V>(ConcurrentDictionary<K, ClientResponse<V>> cache, K key, Func<Task<ClientResponse<V>>> func)
    {
        if (cache.TryGetValue(key, out var response))
        {
            return response;
        }
        else
        {
            response = await func.Invoke();
            if (response.HasValue || response.HasError(e => e.Cachable))
            {
                cache[key] = response;
            }

            return response;
        }
    }
}
