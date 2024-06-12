namespace TomestoneViewer.Character.Encounter.Data;

public class SourceEncounterData<T>
    where T : class
{
    public LoadingStatus Status { get; } = new();

    public T? Data { get; private set; } = null;

    public void StartLoading()
    {
        this.Status.LoadingStarted();
    }

    public void Load(T data)
    {
        this.Status.LoadingDone();
        this.Data = data;
    }

    public void Load(IEncounterDataError error)
    {
        this.Status.LoadingError(error);
    }

    public void Reset()
    {
        this.Status.LoadingStarted();
        this.Data = null;
    }
}
