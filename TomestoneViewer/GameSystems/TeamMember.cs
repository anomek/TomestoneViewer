using TomestoneViewer.Character;

namespace TomestoneViewer.GameSystems;

public record TeamMember(
    CharacterId CharacterId,
    JobId JobId,
    bool IsInParty,
    bool IsLeader)
{
}
