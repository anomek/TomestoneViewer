using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client;

public class Cache<TKey, TValue, TError>
    where TKey : notnull
    where TError : IClientError
{
    private readonly ConcurrentDictionary<TKey, Entry<ClientResponse<TValue, TError>>> cache = [];
    private readonly TimeSpan validity = TimeSpan.FromMinutes(30);

    internal async Task<ClientResponse<TValue, TError>> Get(TKey key, Func<Task<ClientResponse<TValue, TError>>> supplier)
    {
        if (this.cache.TryGetValue(key, out var entry) && entry.IsUpToDate(this.validity))
        {
            return entry.Value;
        }

        var value = await supplier.Invoke();
        if (value.Cachable)
        {
            this.cache[key] = new Entry<ClientResponse<TValue, TError>>(value);
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
