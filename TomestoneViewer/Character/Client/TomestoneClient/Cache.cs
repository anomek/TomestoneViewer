using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client.TomestoneClient;

internal class Cache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Entry<ClientResponse<TValue>>> cache = [];
    private readonly TimeSpan validity = TimeSpan.FromMinutes(30);

    internal async Task<ClientResponse<TValue>> Get(TKey key, Func<Task<ClientResponse<TValue>>> supplier)
    {
        if (this.cache.TryGetValue(key, out var entry) && entry.IsUpToDate(this.validity))
        {
            return entry.Value;
        }

        var value = await supplier.Invoke();
        if (value.Cachable)
        {
            this.cache[key] = new Entry<ClientResponse<TValue>>(value);
        }

        return value;
    }

    internal record Entry<T>(T Value)
    {
        private readonly DateTime timestamp = DateTime.Now;

        internal bool IsUpToDate(TimeSpan validity)
        {
            return this.timestamp + validity > DateTime.Now;
        }
    }
}
