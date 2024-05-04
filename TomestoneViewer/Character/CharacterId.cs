using Dalamud.Game.ClientState.Party;
using TomestoneViewer.Model;

namespace TomestoneViewer.Character;

public record CharacterId(string FirstName, string LastName, string World)
{
    private readonly string firstName = FirstName;
    private readonly string lastName = LastName;
    private readonly string world = World;

    public string FirstName { get => this.firstName; }

    public string LastName { get => this.lastName; }

    public string World { get => this.world; }

    public string FullName { get => $"{this.FirstName} {this.LastName}"; }

    public static CharacterId From(TeamMember partyMember)
    {
        return new CharacterId(partyMember.FirstName, partyMember.LastName, partyMember.World);
    }

    public override string ToString()
    {
        return $"{this.FullName}@{this.World}";
    }
}
