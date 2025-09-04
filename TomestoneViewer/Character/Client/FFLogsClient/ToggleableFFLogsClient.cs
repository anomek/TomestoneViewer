using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

public class ToggleableFFLogsClient(Func<bool> toggle, IFFLogsClient client) : IFFLogsClient
{
    private readonly Func<bool> toggle = toggle;
    private readonly IFFLogsClient client = client;

    public async Task<ClientResponse<FFLogsClientError, FFLogsCharId>> FetchCharacter(CharacterId characterId, CancellationToken cancellationToken)
    {
        if (this.toggle.Invoke())
        {
            return await this.client.FetchCharacter(characterId, cancellationToken);
        }
        else
        {
            return new(FFLogsClientError.Disabled);
        }
    }

    public async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(FFLogsCharId characterId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken)
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
