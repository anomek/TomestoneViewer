using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Dalamud.Game.ClientState.Objects.SubKinds;
using TomestoneViewer.Model;

namespace TomestoneViewer;

public class CharData
{
    private readonly object loadActiveLock = new object();

    public ReadOnlyDictionary<string, EncounterData> EncounterData { get; private set; }

    public CharacterError? CharError { get; set; }

    public CharacterId CharId { get; private set; }

    public uint? LodestoneId { get; set; }

    public uint JobId { get; set; } = 0;

    public bool IsDataLoading { get; set; }

    public bool IsDataReady { get; set; }

    public bool IsActive { get; set; } = true;

    private Task? LoadTask { get; set; }

    public CharData(CharacterId charId)
    {
        this.CharId = charId;
        var encounterData = new Dictionary<string, EncounterData>();
        foreach (var location in EncounterLocation.AllLocations())
        {
            encounterData[location.DisplayName] = new();
        }

        this.EncounterData = encounterData.AsReadOnly();
    }

    public void FetchLogs(string? encounterDisplayName = null)
    {
        Service.PluginLog.Info($"Fetching data for {this.CharId} on {encounterDisplayName}");

        this.SetDataLoading();
        this.SetJobId();
        lock (this.loadActiveLock)
        {
            if (this.LoadTask != null)
            {
                Service.PluginLog.Info($"Skipping loading data for {this.CharId} because it's already being loaded");
                return;
            }

            if (!this.IsActive)
            {
                return;
            }

            var task = Service.TomestoneClient.FetchLogs(this, encounterDisplayName)
                .ContinueWith(t =>
                {
                    this.LoadTask = null;
                    this.SetDataLoaded();

                    Service.PluginLog.Info($"Fetched data with {this.EncounterData.Count} items");

                    if (!t.IsFaulted) return;
                    if (t.Exception == null) return;
                    this.CharError = CharacterError.NetworkError;
                    foreach (var e in t.Exception.Flatten().InnerExceptions)
                    {
                        Service.PluginLog.Error(e, "Network error");
                    }
                });

            if (encounterDisplayName == null)
            {
                this.LoadTask = task;
            }
        }
    }

    public async Task Disable()
    {
        Task? loadTask;
        lock (this.loadActiveLock)
        {
            loadTask = this.LoadTask;
            this.IsActive = false;
        }

        if (loadTask != null)
        {
            await loadTask;
        }
    }

    private void SetDataLoading()
    {
        this.IsDataLoading = true;
        foreach (var location in this.EncounterData.Values)
        {
            location.StartLoading();
        }
    }

    private void SetDataLoaded()
    {
        this.IsDataLoading = false;
        this.IsDataReady = true;
    }

    private void SetJobId()
    {
        // search in the object table first as it updates faster and is always accurate
        for (var i = 0; i < 200; i += 2)
        {
            var obj = Service.ObjectTable[i];
            if (obj != null)
            {
                if (obj is PlayerCharacter playerCharacter
                    && playerCharacter.Name.TextValue == this.CharId.FullName
                    && playerCharacter.HomeWorld.GameData?.Name.RawString == this.CharId.World)
                {
                    this.JobId = playerCharacter.ClassJob.Id;
                    return;
                }
            }
        }

        // if not in object table, search in the team list (can give 0 if normal party member in another zone)
        Service.TeamManager.UpdateTeamList();
        var member = Service.TeamManager.TeamList.FirstOrDefault(member => CharacterId.From(member) == this.CharId);

        if (member != null)
        {
            this.JobId = member.JobId;
            return;
        }

        this.JobId = 0; // avoid stale job id if the current one is not retrievable
    }
}
