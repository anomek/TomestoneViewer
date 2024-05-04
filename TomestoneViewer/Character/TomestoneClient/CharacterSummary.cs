using System.Collections.Generic;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public class CharacterSummary(List<EncounterId> clearedEncounters)
{
    private readonly List<EncounterId> clearedEncounters = clearedEncounters;

    public IEnumerable<EncounterId> ClearedEncounters { get => this.clearedEncounters; }

    public static CharacterSummary Empty()
    {
        return new([]);
    }

    public bool Contains(EncounterId id)
    {
        return this.clearedEncounters.Contains(id);
    }

}
