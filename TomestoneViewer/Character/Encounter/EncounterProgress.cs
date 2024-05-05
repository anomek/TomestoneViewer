using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

public record EncounterProgress
{
    public EncounterClear? EncounterClear { get; }

    public string? Progress { get; }

    public bool Cleared => this.EncounterClear != null;

    private EncounterProgress(EncounterClear? encounterClear, string? progress)
    {
        this.EncounterClear = encounterClear;
        this.Progress = progress;
    }

    public static EncounterProgress EncounterCleared(EncounterClear encounterClear)
    {
        return new EncounterProgress(encounterClear, null);
    }

    public static EncounterProgress EncounterInProgress(string progress)
    {
        return new EncounterProgress(null, progress);
    }

    public static EncounterProgress EncounterNotStarted()
    {
        return new EncounterProgress(null, null);
    }
}
