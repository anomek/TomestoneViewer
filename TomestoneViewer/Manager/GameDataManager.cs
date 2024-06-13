using System;

namespace TomestoneViewer.Manager;

public class GameDataManager : IDisposable
{
    public JobIconsManager JobIconsManager { get; } = new();

    public void Dispose()
    {
        this.JobIconsManager.Dispose();
    }
}
