using TomestoneViewer.Model;

namespace TomestoneViewer.Character;

public record CharacterId(string FirstName, string LastName, string World)
{
    public string FullName => $"{this.FirstName} {this.LastName}";

    public static CharacterId From(TeamMember partyMember)
    {
        return new CharacterId(partyMember.FirstName, partyMember.LastName, partyMember.World);
    }

    public override string ToString()
    {
        return $"{this.FullName}@{this.World}";
    }
}
