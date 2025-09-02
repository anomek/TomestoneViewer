using System;
using System.Threading;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client;

/// <summary>
/// Allows given task to be executed only once at the time
/// Any subsequent calls will wait for running task to finish and will use its results
/// Any calls when no task is running, will run the task.
/// </summary>
internal class SyncQuery<T>(Func<CancellationToken, Task<T>> query)
{
    private readonly Func<CancellationToken, Task<T>> query = query;

    private readonly object syncLock = new();

    private Task<T>? runningTask;

    internal SyncQuery(Func<Task<T>> query)
        : this(tokenIgnored => query.Invoke())
    {
    }

    internal async Task<T> Run()
    {
        return await this.Run(CancellationToken.None);
    }

    internal async Task<T> Run(CancellationToken token)
    {
        var taskLocalCopy = this.runningTask;
        var createdNewTask = false;
        if (taskLocalCopy == null)
        {
            lock (this.syncLock)
            {
                if (this.runningTask == null)
                {
                    this.runningTask = this.query.Invoke(token);
                    createdNewTask = true;
                }

                taskLocalCopy = this.runningTask;
            }
        }

        var result = await taskLocalCopy;
        if (createdNewTask)
        {
            this.runningTask = null;
        }

        return result;
    }
}
