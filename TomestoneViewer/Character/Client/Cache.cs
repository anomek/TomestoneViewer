using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client;

internal class Cache<TKey, TValue, TError>
    where TKey : notnull
    where TError : IClientError
{
    private readonly ConcurrentDictionary<TKey, Entry<ClientResponse<TError, TValue>>> cache = [];
    private readonly TimeSpan validity = TimeSpan.FromMinutes(30);

    internal async Task<ClientResponse<TError, TValue>> Get(TKey key, Func<Task<ClientResponse<TError, TValue>>> supplier)
    {
        if (this.cache.TryGetValue(key, out var entry) && entry.IsUpToDate(this.validity))
        {
            return entry.Value;
        }

        var value = await supplier.Invoke();
        if (value.Cachable)
        {
            this.cache[key] = new Entry<ClientResponse<TError, TValue>>(value);
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
