using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

public interface ITomestoneClient
{
    Task<ClientResponse<TomestoneClientError, LodestoneId>> FetchLodestoneId(CharacterId characterId);

    Task<ClientResponse<TomestoneClientError, CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId);

    Task<ClientResponse<TomestoneClientError, TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, TomestoneLocation location);
}
