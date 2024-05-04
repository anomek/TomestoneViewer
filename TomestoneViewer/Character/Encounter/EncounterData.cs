using TomestoneViewer.Character.TomestoneClient;

namespace TomestoneViewer.Character.Encounter;


public class EncounterData
{
    public CStatus Status { get; private set; } = new();

    public EncounterProgress? EncouterProgress { get; private set; }

    public class CStatus
    {
        public bool Loading { get; private set; } = true;

        public IEncounterDataError? Error { get; private set; } = null;

        internal void LoadingStarted()
        {
            this.Loading = true;
            this.Error = null;
        }

        internal void LoadingDone()
        {
            this.Loading = false;
            this.Error = null;
        }

        internal void LoadingError(IEncounterDataError error)
        {
            this.Loading = false;
            this.Error = error;
        }
    }

    public void StartLoading()
    {
        this.Status.LoadingStarted();
    }

    public void Load(EncounterProgress encounterProgress)
    {
        this.Status.LoadingDone();
        this.EncouterProgress = encounterProgress;
    }

    public void Load(TomestoneClientError error)
    {
        this.Status.LoadingError(error);
    }

    public void Reset()
    {
        this.Status.LoadingStarted();
        this.EncouterProgress = null;
    }
}
