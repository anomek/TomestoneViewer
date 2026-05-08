using System.Threading;
using System.Threading.Tasks;

using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;

public interface IFFLogsClient
{
    Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> FetchEncounter(LodestoneId lodestoneId, FFLogsLocation.FFLogsZone location, CancellationToken cancellationToken);
}
