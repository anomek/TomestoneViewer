namespace TomestoneViewer.Character.TomestoneClient;

public record EncounterProgress
{
    private readonly bool cleared;
    private readonly string? progress;

    public bool Cleared { get => this.cleared; }

    public string? Progress { get => this.progress; }

    private EncounterProgress(bool cleared, string? progress)
    {
        this.cleared = cleared;
        this.progress = progress;
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
