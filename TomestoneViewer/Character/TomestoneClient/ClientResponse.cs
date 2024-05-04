using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.TomestoneClient;

// Kind of either class, I don't want to play with exceptions
public class ClientResponse<T>
{
    private readonly T? value;
    private readonly TomestoneClientError? error;

    public bool HasValue { get => this.value != null; }

    public TomestoneClientError Error { get => (TomestoneClient.TomestoneClientError)this.error; }

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
