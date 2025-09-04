using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client;

// Kind of either class, I don't want to play with exceptions
public class ClientResponse<TError, T>
    where TError : IClientError
{
    private readonly TError? error;
    private readonly T? value;

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

    public async Task<TResult> FoldAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<TError, Task<TResult>> onError)
    {
        if (this.value != null)
        {
            return await onSuccess.Invoke(this.value);
        }
        else if (this.error != null)
        {
            return await onError.Invoke(this.error);
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

    public bool OnSuccess(Action<T> onSuccess)
    {
        return this.IfSuccessOrElse(onSuccess, error => { });
    }

    public bool HasError(Predicate<TError> predicate)
    {
        return this.error != null && predicate.Invoke(this.error);
    }

    public ClientResponse<TError, TResult> Map<TResult>(Func<T, TResult> transform)
    {
        return this.Fold(value => new ClientResponse<TError, TResult>(transform(value)), error => new(error));
    }

    public ClientResponse<TError, TResult> FlatMap<TResult>(Func<T, ClientResponse<TError, TResult>> transform)
    {
        return this.Fold(value => transform(value), error => new(error));
    }

    public async Task<ClientResponse<TError, TResult>> FlatMapAsync<TResult>(Func<T, Task<ClientResponse<TError, TResult>>> transform)
    {
        return await this.FoldAsync(value => transform(value), async error => new(error));
    }

    public ClientResponse<TError, T> Recover(Predicate<TError> onError, Func<ClientResponse<TError, T>> recoveryAction)
    {
        if (this.HasError(onError))
        {
            return recoveryAction.Invoke();
        }
        else
        {
            return this;
        }
    }

    public async Task<ClientResponse<TError, T>> RecoverAsync(Predicate<TError> onError, Func<Task<ClientResponse<TError, T>>> recoveryAction)
    {
        if (this.HasError(onError))
        {
            return await recoveryAction.Invoke();
        }
        else
        {
            return this;
        }
    }

    internal ClientResponse(T value)
    {
        this.value = value;
    }

    internal ClientResponse(TError error)
    {
        this.error = error;
    }

    public static ClientResponse<TError, IReadOnlyList<T>> Collate(IEnumerable<ClientResponse<TError, T>> clientResponses)
    {
        var firstError = clientResponses
            .Where(response => response.error != null)
            .Select(response => response.error)
            .FirstOrDefault();
        if (firstError != null)
        {
            return new(firstError);
        }
        else
        {
            return new(clientResponses
                .Select(response => (T)response.value!)
                .ToList());
        }
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
