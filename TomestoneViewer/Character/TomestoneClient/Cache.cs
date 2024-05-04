using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.TomestoneClient;

internal class Cache<K, V>
{
    private readonly ConcurrentDictionary<K, Entry<ClientResponse<V>>> cache = [];
    private readonly TimeSpan validity = TimeSpan.FromMinutes(30);

    internal async Task<ClientResponse<V>> Get(K key, Func<Task<ClientResponse<V>>> supplier)
    {
        if (this.cache.TryGetValue(key, out var entry) && entry.IsUpToDate(this.validity))
        {
           return entry.Value;
        }

        var value = await supplier.Invoke();
        if (value.Cachable)
        {
            this.cache[key] = new Entry<ClientResponse<V>>(value);
        }

        return value;
    }

    internal record Entry<V>(V Value)
    {
        private readonly DateTime timestamp = DateTime.Now;

        internal bool IsUpToDate(TimeSpan validity)
        {
            return this.timestamp + validity > DateTime.Now;
        }
    }
}
