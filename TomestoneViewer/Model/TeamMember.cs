namespace TomestoneViewer.Model;

public class TeamMember
{
    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string World { get; init; } = null!;

    public uint JobId { get; init; }

    public bool IsInParty { get; init; }
}
