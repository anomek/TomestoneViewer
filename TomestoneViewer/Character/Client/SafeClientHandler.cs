using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client;

internal class SafeClientHandler<TError>(TError defaultError)
    where TError : IClientError
{
    private readonly TError defaultError = defaultError;

    internal ClientResponse<TError, T> HandleTaskErrors<T>(Task<ClientResponse<TError, T>> responseTask)
    {
        if (responseTask.IsFaulted)
        {
            Service.PluginLog.Error(responseTask.Exception, "Exception thrown by client");
            return new(this.defaultError);
        }
        else
        {
            return responseTask.Result;
        }
    }
}
