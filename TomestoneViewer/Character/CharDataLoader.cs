using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character;

internal class CharDataLoader
{
    private readonly CharacterId characterId;
    private readonly IReadOnlyDictionary<Location, EncounterData> encounterData;
    private readonly CancelableTomestoneClient tomestoneClient;
    private readonly CancelableFFLogsClient fflogsClient;

    private LodestoneId? lodestoneId;
    private TomestoneClientError? loadError;

    internal LodestoneId? LodestoneId => this.lodestoneId;

    internal TomestoneClientError? LoadError => this.loadError;

    internal IReadOnlyDictionary<Location, EncounterData> EncounterData => this.encounterData;

    internal CharDataLoader(CharacterId characterId, ITomestoneClient tomestoneClient, IFFLogsClient fflogsClient)
    {
        this.characterId = characterId;
        this.encounterData = Location.All()
            .ToDictionary(location => location, _ => new EncounterData())
            .AsReadOnly();
        this.tomestoneClient = new CancelableTomestoneClient(tomestoneClient);
        this.fflogsClient = new CancelableFFLogsClient(fflogsClient);
    }

    internal void Cancel()
    {
        this.tomestoneClient.Cancel();
        this.fflogsClient.Cancel();
    }

    internal async Task Load(Location? selectedLocation)
    {
        // TODO: move it out of client?
        Service.HistoryManager.AddHistoryEntry(this.characterId);

        // Fetch lodestoneId
        if (this.lodestoneId == null)
        {
            (await this.tomestoneClient.FetchLodestoneId(this.characterId))
                .IfSuccessOrElse(
                lodestoneId => this.lodestoneId = lodestoneId,
                error => this.loadError = error);
        }

        if (this.lodestoneId == null)
        {
            return;
        }

        // Fetch summary
        var summary = (await this.tomestoneClient.FetchCharacterSummary(this.lodestoneId))
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
            .Select(location => this.FetchTomestoneForLocation(summary, location)
                        .ContinueWith((s) => this.FetchFFLogsForLocation(location)))
            .OfType<Task>());
    }

    private async Task FetchFFLogsForLocation(Location location)
    {

        return;
    }

    private async Task FetchTomestoneForLocation(CharacterSummary summary, Location location)
    {
        if (summary.TryGet(location.UltimateId, out var encounterProgress))
        {
            this.encounterData[location].Tomestone.Load(encounterProgress);
            if (encounterProgress.Cleared)
            {
                return;
            }
            else
            {
                await this.tomestoneClient
                    .FetchEncounter(this.lodestoneId, location)
                    .ContinueWith(t => this.Apply(t.Result, location, false));
                return;
            }
        }
        else
        {
            await this.tomestoneClient
                .FetchEncounter(this.lodestoneId, location)
                .ContinueWith(t => this.Apply(t.Result, location, true));
            return;
        }
    }

    private void Apply(ClientResponse<TomestoneEncounterData> encounterProgressResponse, Location location, bool applyErrors)
    {
        encounterProgressResponse.IfSuccessOrElse(
            encounterProgress => this.encounterData[location].Tomestone.Load(encounterProgress),
            error =>
            {
                if (applyErrors)
                {
                    this.encounterData[location].Tomestone.Load(error);
                }
            });
    }
}
