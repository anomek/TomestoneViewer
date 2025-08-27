using TomestoneViewer.Character.TomestoneClient;

namespace TomestoneViewer.Character.Encounter;

public partial class EncounterData
{
    public LoadableData<FFLogsData> FFLogsData { get; private init; } = new();

    public LoadableData<TomestoneData> TomestoneData { get; private init; } = new();

    public class LoadableData<T>
        where T : class
    {
        public CStatus Status { get; private set; } = new();

        public T? Data { get; private set; }

        public void StartLoading()
        {
            this.Status.LoadingStarted();
        }

        public void Load(T data)
        {
            this.Status.LoadingDone();
            this.Data = data;
        }

        public void Load(TomestoneClientError error)
        {
            this.Status.LoadingError(error);
        }

        public void Reset()
        {
            this.Status.LoadingStarted();
            this.Data = null;
        }
    }

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
}
