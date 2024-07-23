using System;

namespace TomestoneViewer.Character.TomestoneClient;

// Kind of either class, I don't want to play with exceptions
public class ClientResponse<T>
{
    private readonly T? value;
    private readonly TomestoneClientError? error;

    internal bool Cachable => this.error == null || this.error.Cachable;

    public TResult Fold<TResult>(Func<T, TResult> onSuccess, Func<TomestoneClientError, TResult> onError)
    {
        if (this.value != null)
        {
            return onSuccess.Invoke(this.value);
        }
        else if (this.error != null)
        {
            return onError.Invoke(this.error);
        }
        else
        {
            throw new System.InvalidOperationException("Improperly initialized object");
        }
    }

    public bool IfSuccessOrElse(Action<T> onSuccess, Action<TomestoneClientError> onError)
    {
        return this.Fold(ToFunc(onSuccess, true), ToFunc(onError, false));
    }

    public bool HasError(Predicate<TomestoneClientError> predicate)
    {
        return this.error != null && predicate.Invoke(this.error);
    }

    public ClientResponse<TResult> FlatMap<TResult>(Func<T, ClientResponse<TResult>> transform)
    {
        return this.Fold(value => transform(value), error => new(error));
    }

    internal ClientResponse(T value)
    {
        this.value = value;
    }

    internal ClientResponse(TomestoneClientError error)
    {
        this.error = error;
    }

    private static Func<TIn, TOut> ToFunc<TIn, TOut>(Action<TIn> action, TOut value)
    {
        return input =>
        {
            action(input);
            return value;
        };
    }
}
