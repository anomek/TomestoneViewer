using System;

using TomestoneViewer.Character;

namespace TomestoneViewer.Model;

public record HistoryEntry(
    string FirstName,
    string LastName,
    string WorldName)
{
    public DateTime LastSeen { get; set; } = DateTime.Now;

    public CharacterId CharId
    {
        get
        {
            return new CharacterId(this.FirstName, this.LastName, this.WorldName);
        }
    }

    public static HistoryEntry From(CharacterId characterId)
    {
        return new HistoryEntry(characterId.FirstName, characterId.LastName, characterId.World);
    }
}
