using System.Collections.Generic;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character;

public class CharData
{
    private readonly CharacterId characterId;
    private readonly CharDataLoader loader;
    private readonly Links links;

    public IReadOnlyDictionary<Location, EncounterData> EncounterData => this.loader.EncounterData;

    public IEncounterDataError? CharError => this.loader.LoadError;

    public CharacterId CharId => this.characterId;

    public Links Links { get => this.links; }

    public JobId JobId { get; set; } = JobId.Empty;

    internal CharData(CharacterId characterId, CharDataLoader loader)
    {
        this.characterId = characterId;
        this.loader = loader;
        this.links = new Links(characterId, loader);
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
