using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

public interface IFFLogsClient
{
    Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(CharacterId characterId, FFLogsLocation location);
}
