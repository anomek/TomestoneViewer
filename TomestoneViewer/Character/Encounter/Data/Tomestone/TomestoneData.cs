namespace TomestoneViewer.Character.Encounter.Data.Tomestone;

public record TomestoneData
{
    public EncounterClear? EncounterClear { get; }

    public ProgPoint? Progress { get; }

    public bool Cleared => this.EncounterClear != null;

    private TomestoneData(EncounterClear? encounterClear, ProgPoint? progress)
    {
        this.EncounterClear = encounterClear;
        this.Progress = progress;
    }

    public static TomestoneData EncounterCleared(EncounterClear encounterClear)
    {
        return new TomestoneData(encounterClear, null);
    }

    public static TomestoneData EncounterInProgress(ProgPoint progress)
    {
        return new TomestoneData(null, progress);
    }

    public static TomestoneData EncounterNotStarted()
    {
        return new TomestoneData(null, null);
    }
}
