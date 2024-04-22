using System;

namespace TomestoneViewer.Model;

public class HistoryEntry
{
    public string FirstName = null!;
    public string LastName = null!;
    public string WorldName = null!;
    public DateTime LastSeen = DateTime.Now;

    public CharacterId CharId
    {
        get
        {
            return new CharacterId(this.FirstName, this.LastName, this.WorldName);
        }
    }

    public static HistoryEntry From(CharacterId characterId)
    {
        return new HistoryEntry()
        {
            FirstName = characterId.FirstName,
            LastName = characterId.LastName,
            WorldName = characterId.World,
        };
    }
}
