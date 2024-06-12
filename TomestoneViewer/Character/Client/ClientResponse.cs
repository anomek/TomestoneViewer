using System;

namespace TomestoneViewer.Character.Client;

// Kind of either class, I don't want to play with exceptions
public class ClientResponse<T, TError>
    where TError : IClientError
{
    private readonly T? value;
    private readonly TError? error;

    internal bool Cachable => this.error == null || this.error.Cachable;

    public TResult Fold<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onError)
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
            throw new InvalidOperationException("Improperly initialized object");
        }
    }

    public bool IfSuccessOrElse(Action<T> onSuccess, Action<TError> onError)
    {
        return this.Fold(ToFunc(onSuccess, true), ToFunc(onError, false));
    }

    public bool HasError(Predicate<TError> predicate)
    {
        return this.error != null && predicate.Invoke(this.error);
    }

    public ClientResponse<TResult, TError> FlatMap<TResult>(Func<T, ClientResponse<TResult, TError>> transform)
    {
        return this.Fold(value => transform(value), error => new(error));
    }

    internal ClientResponse(T value)
    {
        this.value = value;
    }

    internal ClientResponse(TError error)
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
