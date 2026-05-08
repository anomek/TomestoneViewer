using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

public record CharacterSummary(IReadOnlyDictionary<TomestoneLocationId, TomestoneEncounterData> EncountersProgres, CharacterId? MainCharacter)
{
    public static CharacterSummary Empty()
    {
        return new(new Dictionary<TomestoneLocationId, TomestoneEncounterData>().AsReadOnly(), null);
    }

    public bool TryGet(TomestoneLocationId? id, [MaybeNullWhen(false)] out TomestoneEncounterData encounterClear)
    {
        if (id != null && this.EncountersProgres.ContainsKey(id))
        {
            encounterClear = this.EncountersProgres[id];
            return true;
        }
        else
        {
            encounterClear = null;
            return false;
        }
    }
}
