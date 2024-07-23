namespace TomestoneViewer.Character.Encounter;

/// <summary>
/// Ultimate Id according to tomestone.
/// </summary>
public record UltimateId(int Id)
{
    public override string ToString()
    {
        return this.Id.ToString();
    }
}
