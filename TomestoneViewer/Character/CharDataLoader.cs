using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.TomestoneClient;

namespace TomestoneViewer.Character;

internal class CharDataLoader
{
    private readonly CharacterId characterId;
    private readonly ReadOnlyDictionary<string, EncounterData> encounterData;
    private readonly CancelableTomestoneClient client;
    private LodestoneId? lodestoneId;
    private TomestoneClientError? loadError;

    internal TomestoneClientError? LoadError => this.loadError;

    internal ReadOnlyDictionary<string, EncounterData> EncounterData => this.encounterData;

    internal CharDataLoader(CharacterId characterId)
    {
        this.characterId = characterId;
        var encounterData = new Dictionary<string, EncounterData>();
        foreach (var location in EncounterLocation.AllLocations())
        {
            encounterData[location.DisplayName] = new();
        }

        this.encounterData = encounterData.AsReadOnly();
        this.client = new(Service.TomestoneClient);
    }

    internal void Cancel()
    {
        this.client.Cancel();
    }


    internal async Task Load(string? encounterDisplayName)
    {
        // TODO: move it out of client?
        Service.HistoryManager.AddHistoryEntry(this.characterId);

        if (this.lodestoneId == null)
        {
            var lodestoneIdResponse = await Service.TomestoneClient.FetchLodestoneId(this.characterId);
            if (lodestoneIdResponse.TryGetValue(out var lodestoneId))
            {
                this.lodestoneId = lodestoneId;
            }
            else
            {
                this.loadError = lodestoneIdResponse.Error;
                return;
            }
        }

        var summaryResponse = await Service.TomestoneClient.FetchCharacterSummary(this.lodestoneId);
        if (!summaryResponse.TryGetValue(out var summary) && !summaryResponse.Error.CanIgnore)
        {
            this.loadError = summaryResponse.Error;
            return;
        }

        summary ??= CharacterSummary.Empty();

        List<Task> tasks = [];
        foreach (var category in EncounterLocation.LOCATIONS)
        {
            foreach (var location in category.Locations)
            {
                if (location.Id != null && summary.Contains(location.Id))
                {
                    this.encounterData[location.DisplayName].Load(EncounterProgress.EncounterCleared(summary.ClearedEncounters[location.Id]));
                }
                else if (encounterDisplayName == null || encounterDisplayName.Equals(location.DisplayName))
                {
                    var loopEncountrDisplayName = location.DisplayName;
                    tasks.Add(Service.TomestoneClient.FetchEncounter(this.lodestoneId, category, location)
                        .ContinueWith(t => this.Apply(t.Result, loopEncountrDisplayName)));
                }
            }
        }

        await Task.WhenAll(tasks);
    }

    private void Apply(ClientResponse<EncounterProgress> encounterProgressResponse, string encounterDisplayName)
    {
        if (encounterProgressResponse.TryGetValue(out var encounterProgress))
        {
            this.encounterData[encounterDisplayName].Load(encounterProgress);
        }
        else
        {
            this.encounterData[encounterDisplayName].Load(encounterProgressResponse.Error);
        }
    }
}
