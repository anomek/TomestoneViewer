using System.Collections.Generic;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character;

public class CharData
{
    private readonly CharacterId characterId;
    private readonly CharDataLoader loader;

    public IReadOnlyDictionary<Location, EncounterData> EncounterData => this.loader.EncounterData;

    public IEncounterDataError? CharError => this.loader.LoadError;

    public CharacterId CharId => this.characterId;

    public JobId JobId { get; set; } = JobId.Empty;

    internal CharData(CharacterId characterId, CharDataLoader loader)
    {
        this.characterId = characterId;
        this.loader = loader;
    }

    public void FetchLogs(Location? encounterDisplayName = null)
    {
        _ = this.loader.Load(encounterDisplayName);
    }

    public void Disable()
    {
        this.loader.Cancel();
    }
}
