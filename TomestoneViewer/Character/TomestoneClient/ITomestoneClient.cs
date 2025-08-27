using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public interface ITomestoneClient
{
    Task<ClientResponse<LodestoneId>> FetchLodestoneId(CharacterId characterId);

    Task<ClientResponse<CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId);

    Task<ClientResponse<TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, Location location);
}
