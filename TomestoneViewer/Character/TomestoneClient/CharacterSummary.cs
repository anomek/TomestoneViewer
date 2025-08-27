using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public record CharacterSummary(IReadOnlyDictionary<UltimateId, TomestoneEncounterData> UltimatesProgress)
{
    public static CharacterSummary Empty()
    {
        return new(new Dictionary<UltimateId, TomestoneEncounterData>().AsReadOnly());
    }

    public bool TryGet(UltimateId? id, [MaybeNullWhen(false)] out TomestoneEncounterData encounterClear)
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
