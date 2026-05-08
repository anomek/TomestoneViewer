using System;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client;

internal class SyncValue<T>(Func<Task<T>> query)
{
    private readonly Func<Task<T>> query = query;
    private readonly object syncLock = new();

    private Task<T>? task;
    private T? value;

    internal async Task<T> Get()
    {
        Task<T>? taskLocalCopy = null;
        if (this.value != null)
        {
            return this.value;
        }
        else
        {
            lock (this.syncLock)
            {
                if (this.value == null)
                {
                    this.task ??= this.query();
                    taskLocalCopy = this.task;
                }
                else
                {
                    return this.value;
                }
            }
        }

        this.value = await taskLocalCopy;
        return this.value;
    }

    internal void Clear()
    {
        lock (this.syncLock)
        {
            if (this.task == null)
            {
                this.value = default(T);
            }
        }
    }
}
