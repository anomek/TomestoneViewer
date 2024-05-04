using System;
using System.Diagnostics.CodeAnalysis;

namespace TomestoneViewer.Character.TomestoneClient;

// Kind of either class, I don't want to play with exceptions
public class ClientResponse<T>
{
    private readonly T? value;
    private readonly TomestoneClientError? error;

    public bool HasValue => this.value != null;

    internal bool Cachable => this.error == null || this.error.Cachable;

    public TomestoneClientError Error => (TomestoneClient.TomestoneClientError)this.error;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        if (this.value == null)
        {
            value = default;
            return false;
        }
        else
        {
            value = this.value;
            return true;
        }
    }

    public bool HasError(Predicate<TomestoneClientError> predicate)
    {
        return this.error != null && predicate.Invoke(this.Error);
    }

    public ClientResponse<R> FlatMap<R>(Func<T, ClientResponse<R>> transform)
    {
        if (this.TryGetValue(out var value))
        {
            return transform.Invoke(value);
        }
        else
        {
            return new(this.Error);
        }
    }

    internal ClientResponse(T value)
    {
        this.value = value;
    }

    internal ClientResponse(TomestoneClientError error)
    {
        this.error = error;
    }
}
