namespace TomestoneViewer.Character.Encounter;

public record TomestoneEncounterData
{
    public EncounterClear? EncounterClear { get; }

    public ProgPoint? Progress { get; }

    public bool Cleared => this.EncounterClear != null;

    private TomestoneEncounterData(EncounterClear? encounterClear, ProgPoint? progress)
    {
        this.EncounterClear = encounterClear;
        this.Progress = progress;
    }

    public static TomestoneEncounterData EncounterCleared(EncounterClear encounterClear)
    {
        return new TomestoneEncounterData(encounterClear, null);
    }

    public static TomestoneEncounterData EncounterInProgress(ProgPoint progress)
    {
        return new TomestoneEncounterData(null, progress);
    }

    public static TomestoneEncounterData EncounterNotStarted()
    {
        return new TomestoneEncounterData(null, null);
    }
}
