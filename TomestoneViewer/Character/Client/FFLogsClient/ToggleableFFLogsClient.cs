using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

public class ToggleableFFLogsClient(Func<bool> toggle, IFFLogsClient client) : IFFLogsClient
{
    private readonly Func<bool> toggle = toggle;
    private readonly IFFLogsClient client = client;


    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(LodestoneId characterId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
    {
        if (this.toggle.Invoke())
        {
            return await this.client.FetchEncounter(characterId, location, cancellationToken);
        }
        else
        {
            return new(FFLogsClientError.Disabled);
        }
    }
}
