using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.TomestoneClient;

namespace TomestoneViewer.Character;

internal class CharDataLoader
{
    private readonly CharacterId characterId;
    private readonly IReadOnlyDictionary<Location, EncounterData> encounterData;
    private readonly CancelableTomestoneClient client;

    private LodestoneId? lodestoneId;
    private TomestoneClientError? loadError;

    internal LodestoneId? LodestoneId => this.lodestoneId;

    internal TomestoneClientError? LoadError => this.loadError;

    internal IReadOnlyDictionary<Location, EncounterData> EncounterData => this.encounterData;

    internal CharDataLoader(CharacterId characterId, ITomestoneClient client)
    {
        this.characterId = characterId;
        this.encounterData = Location.All()
            .ToDictionary(location => location, _ => new EncounterData())
            .AsReadOnly();
        this.client = new CancelableTomestoneClient(client);
    }

    internal void Cancel()
    {
        this.client.Cancel();
    }

    internal async Task Load(Location? selectedLocation)
    {
        // TODO: move it out of client?
        Service.HistoryManager.AddHistoryEntry(this.characterId);

        // Fetch lodestoneId
        if (this.lodestoneId == null)
        {
            (await this.client.FetchLodestoneId(this.characterId))
                .IfSuccessOrElse(
                lodestoneId => this.lodestoneId = lodestoneId,
                error => this.loadError = error);
        }

        if (this.lodestoneId == null)
        {
            return;
        }

        // Fetch summary
        var summary = (await this.client.FetchCharacterSummary(this.lodestoneId))
            .Fold(
            summaryResponse => summaryResponse,
            error =>
            {
                if (error.CanIgnore)
                {
                    return CharacterSummary.Empty();
                }
                else
                {
                    this.loadError = error;
                    return null;
                }
            });

        if (summary == null)
        {
            return;
        }

        // Fetch locations
        await Task.WhenAll(
            (selectedLocation == null ? Location.All() : [selectedLocation])
            .Select(location =>
            {
                if (summary.TryGet(location.UltimateId, out var encounterProgress))
                {
                    this.encounterData[location].Load(encounterProgress);
                    if (encounterProgress.Cleared)
                    {
                        return null;
                    }
                    else
                    {
                        return this.client
                            .FetchEncounter(this.lodestoneId, location)
                            .ContinueWith(t => this.Apply(t.Result, location, false));
                    }
                }
                else
                {
                    return this.client
                        .FetchEncounter(this.lodestoneId, location)
                        .ContinueWith(t => this.Apply(t.Result, location, true));
                }
            })
            .OfType<Task>());
    }

    private void Apply(ClientResponse<EncounterProgress> encounterProgressResponse, Location location, bool applyErrors)
    {
        encounterProgressResponse.IfSuccessOrElse(
            encounterProgress => this.encounterData[location].Load(encounterProgress),
            error =>
            {
                if (applyErrors)
                {
                    this.encounterData[location].Load(error);
                }
            });
    }
}
