using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Encounter.Data.Tomestone;

namespace TomestoneViewer.Character.Client.TomestoneClient;

public record CharacterSummary(IReadOnlyDictionary<UltimateId, TomestoneData> UltimatesProgress)
{
    public static CharacterSummary Empty()
    {
        return new(new Dictionary<UltimateId, TomestoneData>().AsReadOnly());
    }

    public bool TryGet(UltimateId? id, [MaybeNullWhen(false)] out TomestoneData encounterClear)
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
