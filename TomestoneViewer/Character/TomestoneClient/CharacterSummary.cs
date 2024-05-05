using System.Collections.Generic;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public record CharacterSummary(IReadOnlyDictionary<EncounterId, EncounterClear> ClearedEncounters)
{
    public static CharacterSummary Empty()
    {
        return new(new Dictionary<EncounterId, EncounterClear>().AsReadOnly());
    }

    public bool Contains(EncounterId id)
    {
        return this.ClearedEncounters.ContainsKey(id);
    }

}
