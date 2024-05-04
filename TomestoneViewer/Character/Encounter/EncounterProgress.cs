namespace TomestoneViewer.Character.TomestoneClient;

public record EncounterProgress
{
    public bool Cleared { get; }

    public string? Progress { get; }

    private EncounterProgress(bool cleared, string? progress)
    {
        this.Cleared = cleared;
        this.Progress = progress;
    }

    public static EncounterProgress EncounterCleared()
    {
        return new EncounterProgress(true, null);
    }

    public static EncounterProgress EncounterInProgress(string progress)
    {
        return new EncounterProgress(false, progress);
    }

    public static EncounterProgress EncounterNotStarted()
    {
        return new EncounterProgress(false, null);
    }
}
