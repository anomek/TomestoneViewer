using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;

internal class SafeTomestoneClient(ITomestoneClient client) : ITomestoneClient
{
    private readonly ITomestoneClient client = client;

    public Task<ClientResponse<CharacterSummary>> FetchCharacterSummary(LodestoneId lodestoneId)
    {
        return this.client.FetchCharacterSummary(lodestoneId)
            .ContinueWith(this.HandleTaskErrors);
    }

    public Task<ClientResponse<EncounterProgress>> FetchEncounter(LodestoneId lodestoneId, EncounterLocation.Category category, EncounterLocation location)
    {
        return this.client.FetchEncounter(lodestoneId, category, location)
            .ContinueWith(this.HandleTaskErrors);
    }

    public Task<ClientResponse<LodestoneId>> FetchLodestoneId(CharacterId characterId)
    {
        return this.client.FetchLodestoneId(characterId)
            .ContinueWith(this.HandleTaskErrors);
    }

    private ClientResponse<T> HandleTaskErrors<T>(Task<ClientResponse<T>> responseTask)
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
