using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

public interface IFFLogsClient
{
    Task<ClientResponse<FFLogsClientError, FFLogsCharId>> FetchCharacter(CharacterId characterId, CancellationToken cancellationToken);

    Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(FFLogsCharId characterId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken);
}
