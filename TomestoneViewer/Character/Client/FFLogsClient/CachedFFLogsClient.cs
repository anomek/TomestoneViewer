using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class CachedFFLogsClient(string cachePath, IFFLogsClient client) : IFFLogsClient
{ 
    private readonly IFFLogsClient client = client;

    private readonly Cache<(LodestoneId, FFLogsLocation.FFLogsZone), FFLogsEncounterData, FFLogsClientError> encounterCache = new();
    private readonly PersistentFFLogsCache persisntentEncounterCache = new(cachePath);

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(LodestoneId lodestoneId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        if (location.PreviousExpansion)
        {
            return await this.persisntentEncounterCache.Get(lodestoneId, location.BossId, () => this.client.FetchEncounter(lodestoneId, location, cancellationToken));
        }
        else
        { 
            return await this.encounterCache.Get((lodestoneId, location), () => this.client.FetchEncounter(lodestoneId, location, cancellationToken));
        }
    }
}
