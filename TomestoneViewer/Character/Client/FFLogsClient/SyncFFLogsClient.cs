using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;
internal class SyncFFLogsClient(IFFLogsClient client) : IFFLogsClient
{
    private readonly IFFLogsClient client = client;
    private readonly ConcurrentDictionary<(LodestoneId, FFLogsLocation.FFLogsZone), SyncQuery<ClientResponse<FFLogsClientError, FFLogsEncounterData>>> encounters = [];

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(LodestoneId characterId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        return await this.encounters.GetOrAdd((characterId, location), arg => new SyncQuery<ClientResponse<FFLogsClientError, FFLogsEncounterData>>(token => this.client.FetchEncounter(arg.Item1, arg.Item2, token)))
            .Run(cancellationToken);
    }
}
