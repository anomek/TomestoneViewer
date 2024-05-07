using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public record CharacterSummary(IReadOnlyDictionary<UltimateId, EncounterClear> ClearedEncounters)
{
    public static CharacterSummary Empty()
    {
        return new(new Dictionary<UltimateId, EncounterClear>().AsReadOnly());
    }

    public bool TryGet(UltimateId? id, [MaybeNullWhen(false)] out EncounterClear encounterClear)
    {
        if (id != null && this.ClearedEncounters.ContainsKey(id))
        {
            encounterClear = this.ClearedEncounters[id];
            return true;
        }
        else
        {
            encounterClear = null;
            return false;
        }
    }
}
