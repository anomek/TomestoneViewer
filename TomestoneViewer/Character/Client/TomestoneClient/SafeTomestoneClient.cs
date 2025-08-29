using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

internal class SafeTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    public Task<ClientResponse<TomestoneClientError, CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return this.client.FetchCharacterSummary(lodestoneId)
            .ContinueWith(HandleTaskErrors);
    }

    public Task<ClientResponse<TomestoneClientError, TomestoneEncounterData>> FetchEncounter(LodestoneId lodestoneId, TomestoneLocation location)
    {
        return this.client.FetchEncounter(lodestoneId, location)
            .ContinueWith(HandleTaskErrors);
    }

    public Task<ClientResponse<TomestoneClientError, LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return this.client.FetchLodestoneId(characterId)
            .ContinueWith(HandleTaskErrors);
    }

    private static ClientResponse<TomestoneClientError, T> HandleTaskErrors<T>(Task<ClientResponse<TomestoneClientError, T>> responseTask)
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
