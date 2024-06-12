using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Encounter.Data.Tomestone;

namespace TomestoneViewer.Character.Client.TomestoneClient;

internal class SafeTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    public Task<ClientResponse<CharacterSummary, TomestoneClientError>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return this.client.FetchCharacterSummary(lodestoneId)
            .ContinueWith(HandleTaskErrors);
    }

    public Task<ClientResponse<TomestoneData, TomestoneClientError>> FetchEncounter(LodestoneId lodestoneId, Location location)
    {
        return this.client.FetchEncounter(lodestoneId, location)
            .ContinueWith(HandleTaskErrors);
    }

    public Task<ClientResponse<LodestoneId, TomestoneClientError>> FetchLodestoneId(CharacterId characterId)
    {
        return this.client.FetchLodestoneId(characterId)
            .ContinueWith(HandleTaskErrors);
    }

    private static ClientResponse<T, TomestoneClientError> HandleTaskErrors<T>(Task<ClientResponse<T, TomestoneClientError>> responseTask)
    {
        if (responseTask.IsFaulted)
        {
            Service.PluginLog.Error(responseTask.Exception, "Exception thrown by TomestoneClient");
            return new(TomestoneClientError.InternalError);
        }
        else
        {
            return responseTask.Result;
        }
    }
}
