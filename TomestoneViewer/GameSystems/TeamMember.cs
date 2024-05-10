using TomestoneViewer.Character;

namespace TomestoneViewer.GameSystems;

public record TeamMember(
    CharacterId CharacterId,
    uint JobId,
    bool IsInParty,
    bool IsLeader)
{
}
