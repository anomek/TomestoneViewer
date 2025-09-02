using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client;

// Kind of either class, I don't want to play with exceptions
public static class ClientResponseExtensions
{

    public static async Task<ClientResponse<TError, T>> RecoverAsync<TError, T>(this Task<ClientResponse<TError, T>> response, Predicate<TError> onError, Func<Task<ClientResponse<TError, T>>> recoveryAction)
        where TError : IClientError
    {
        return await (await response).RecoverAsync(onError, recoveryAction);
    }

    public static async Task<ClientResponse<TError, TResult>> FlatMapAsync<TError, T, TResult>(this Task<ClientResponse<TError, T>> response, Func<T, Task<ClientResponse<TError, TResult>>> transform)
        where TError : IClientError
    {
        return await (await response).FlatMapAsync(transform);
    }

    public static async Task<ClientResponse<TError, TResult>> Map<TError, T, TResult>(this Task<ClientResponse<TError, T>> response, Func<T, TResult> transform)
        where TError : IClientError
    {
        return (await response).Map(transform);
    }

    public static ClientResponse<TError, IReadOnlyList<T>> Collate<TError, T>(this IEnumerable<ClientResponse<TError, T>> clientResponses)
        where TError : IClientError
    {
       return ClientResponse<TError, T>.Collate(clientResponses);
    }
}
