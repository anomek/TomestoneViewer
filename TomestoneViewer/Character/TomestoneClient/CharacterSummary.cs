using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public record CharacterSummary(IReadOnlyDictionary<UltimateId, EncounterProgress> UltimatesProgress)
{
    public static CharacterSummary Empty()
    {
        return new(new Dictionary<UltimateId, EncounterProgress>().AsReadOnly());
    }

    public bool TryGet(UltimateId? id, [MaybeNullWhen(false)] out EncounterProgress encounterClear)
    {
        if (id != null && this.UltimatesProgress.ContainsKey(id))
        {
            encounterClear = this.UltimatesProgress[id];
            return true;
        }
        else
        {
            encounterClear = null;
            return false;
        }
    }
}
