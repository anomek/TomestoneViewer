using Dalamud.Game.ClientState.Party;

namespace TomestoneViewer.Model;

public record CharacterId(string FirstName, string LastName, string World)
{
    public string FirstName { get; } = FirstName;

    public string LastName { get; } = LastName;

    public string World { get; } = World;

    public string FullName
    {
        get
        {
            return $"{this.FirstName} {this.LastName}";
        }
    }

    public override string ToString()
    {
        return $"{this.FullName}@{this.World}";
    }

    public static CharacterId From(TeamMember partyMember)
    {
        return new CharacterId(partyMember.FirstName, partyMember.LastName, partyMember.World);
    }
}
