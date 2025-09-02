using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class CachedFFLogsClient(IFFLogsClient client) : IFFLogsClient
{ 
    private readonly IFFLogsClient client = client;

    private readonly Cache<CharacterId, FFLogsCharId, FFLogsClientError> characterCache = new();
    private readonly Cache<(FFLogsCharId, FFLogsLocation.FFLogsZone), FFLogsEncounterData, FFLogsClientError> encounterCache = new();

    public async Task<ClientResponse<FFLogsClientError, FFLogsCharId>> FetchCharacter(CharacterId characterId, CancellationToken cancellationToken)
    {
        return await this.characterCache.Get(characterId, () => this.client.FetchCharacter(characterId, cancellationToken));
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(FFLogsCharId characterId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        return await this.encounterCache.Get((characterId, location), () => this.client.FetchEncounter(characterId, location, cancellationToken));
    }
}
