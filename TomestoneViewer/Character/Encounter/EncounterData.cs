using TomestoneViewer.Character.Client;
using TomestoneViewer.Character.Client.TomestoneClient;

namespace TomestoneViewer.Character.Encounter;

public partial class EncounterData
{
    public LoadableData<FFLogsEncounterData> FFLogs { get; private init; } = new();

    public LoadableData<TomestoneEncounterData> Tomestone { get; private init; } = new();

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

        public void Load(IClientError error)
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
