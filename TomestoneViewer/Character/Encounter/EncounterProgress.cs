using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public record EncounterProgress
{
    public EncounterClear? EncounterClear { get; }

    public ProgPoint? Progress { get; }

    public bool Cleared => this.EncounterClear != null;

    private EncounterProgress(EncounterClear? encounterClear, ProgPoint? progress)
    {
        this.EncounterClear = encounterClear;
        this.Progress = progress;
    }

    public static EncounterProgress EncounterCleared(EncounterClear encounterClear)
    {
        return new EncounterProgress(encounterClear, null);
    }

    public static EncounterProgress EncounterInProgress(ProgPoint progress)
    {
        return new EncounterProgress(null, progress);
    }

    public static EncounterProgress EncounterNotStarted()
    {
        return new EncounterProgress(null, null);
    }
}
