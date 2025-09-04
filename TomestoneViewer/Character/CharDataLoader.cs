using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Client;
using System.Threading;

namespace TomestoneViewer.Character;

internal class CharDataLoader
{
    private readonly CharacterId characterId;
    private readonly IReadOnlyDictionary<Location, EncounterData> encounterData;
    private readonly CancelableTomestoneClient tomestoneClient;
    private readonly CancelableFFLogsClient fflogsClient;

    private LodestoneId? lodestoneId;
    private FFLogsCharId? ffLogsCharId;
    private TomestoneClientError? tomestoneLoadError;
    private FFLogsClientError? fflogsLoadError;

    internal LodestoneId? LodestoneId => this.lodestoneId;

    internal TomestoneClientError? GenericTomestoneError => this.tomestoneLoadError;
    internal FFLogsClientError? GenericFFLogsError => this.fflogsLoadError;

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
                error => this.tomestoneLoadError = error);
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
                    this.tomestoneLoadError = error;
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
        if (this.ffLogsCharId == null)
        {
            (await this.fflogsClient.FetchCharacter(this.characterId, CancellationToken.None))
                .IfSuccessOrElse(
                    charId =>
                    {
                        this.fflogsLoadError = null;
                        this.ffLogsCharId = charId;
                    },
                    error => this.fflogsLoadError = error
                );
        }

        if (this.ffLogsCharId != null &&
           (this.encounterData[location].Tomestone.Data == null || this.encounterData[location].Tomestone.Data.Cleared))
        {
            // sequential processing to avoid too many reqeusts - TODO: refactor, not needed any more
            var results = new List<ClientResponse<FFLogsClientError, FFLogsEncounterData>>();
            foreach (var zone in location.FFLogs.Zones)
            {
                results.Add(await this.fflogsClient.FetchEncounter(this.ffLogsCharId, zone, CancellationToken.None));
            }

            this.ApplyFFLogs(results, location, true);
        }
        return;
    }

    private async Task FetchTomestoneForLocation(CharacterSummary summary, Location location)
    {
        if (summary.TryGet(location.Tomestone.UltimateId, out var encounterProgress))
        {
            this.encounterData[location].Tomestone.Load(encounterProgress);
            if (encounterProgress.Cleared)
            {
                return;
            }
            else
            {
                await this.tomestoneClient
                    .FetchEncounter(this.lodestoneId, location.Tomestone)
                    .ContinueWith(t => this.ApplyTomestone(t.Result, location, false));
                return;
            }
        }
        else
        {
            await this.tomestoneClient
                .FetchEncounter(this.lodestoneId, location.Tomestone)
                .ContinueWith(t => this.ApplyTomestone(t.Result, location, true));
            return;
        }
    }

    private void ApplyTomestone(ClientResponse<TomestoneClientError, TomestoneEncounterData> encounterProgressResponse, Location location, bool applyErrors)
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

    private void ApplyFFLogs(IReadOnlyList<ClientResponse<FFLogsClientError, FFLogsEncounterData>> encounterProgressResponse, Location location, bool applyErrors)
    {

        ClientResponse<FFLogsClientError, FFLogsEncounterData>.Collate(encounterProgressResponse)
            .IfSuccessOrElse(
            encounterProgress => this.encounterData[location].FFLogs.Load(FFLogsEncounterData.Compile(encounterProgress)),
            error =>
            {
                if (applyErrors)
                {
                    this.encounterData[location].FFLogs.Load(error);
                }
            });
    }
}
