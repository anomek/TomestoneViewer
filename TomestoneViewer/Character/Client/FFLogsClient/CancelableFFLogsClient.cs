namespace TomestoneViewer.Character.Client.FFLogsClient;

internal class CancelableFFLogsClient(IFFLogsClient client) : IFFLogsClient
{
    private readonly IFFLogsClient client = client;
    private bool canceled;

    public void Cancel()
    {
        this.canceled = true;
    }
}
