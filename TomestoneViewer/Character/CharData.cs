using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.ClientState.Objects.SubKinds;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.GameSystems;

namespace TomestoneViewer.Character;

public class CharData(CharacterId characterId)
{
    private readonly CharacterId characterId = characterId;
    private readonly CharDataLoader loader = new(characterId);

    public IReadOnlyDictionary<Location, EncounterData> EncounterData => this.loader.EncounterData;

    public IEncounterDataError? CharError => this.loader.LoadError;

    public CharacterId CharId => this.characterId;

    public uint JobId { get; set; } = 0;

    public void FetchLogs(Location? encounterDisplayName = null)
    {
        _ = this.loader.Load(encounterDisplayName);
    }

    public void Disable()
    {
        this.loader.Cancel();
    }
}
