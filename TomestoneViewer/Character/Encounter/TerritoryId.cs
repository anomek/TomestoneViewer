namespace TomestoneViewer.Character.Encounter;

/// <summary>
/// ingame Territory Id (row in an TerritoryType excel).
/// </summary>
public record TerritoryId(uint Id)
{
    public static readonly TerritoryId Empty = new(0);

    public override string ToString()
    {
        return this.Id.ToString();
    }
}
