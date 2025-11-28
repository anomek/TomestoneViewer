using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;
internal class SafeFFLogsClient(IFFLogsClient client) : IFFLogsClient
{
    private readonly SafeClientHandler<FFLogsClientError> handler = new(FFLogsClientError.InternalError);
    private readonly IFFLogsClient client = client;

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(LodestoneId characterId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        return await this.client.FetchEncounter(characterId, location, cancellationToken)
            .ContinueWith(this.handler.HandleTaskErrors);
    }
}
