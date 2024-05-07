using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.ClientState.Objects.SubKinds;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character;

public class CharData(CharacterId characterId)
{
    private readonly CharacterId characterId = characterId;
    private readonly CharDataLoader loader = new(characterId);

    public IReadOnlyDictionary<Location, EncounterData> EncounterData => this.loader.EncounterData;

    public IEncounterDataError? CharError => this.loader.LoadError;

    public CharacterId CharId => this.characterId;

    public uint JobId { get; set; } = 0;

    public void FetchLogs(Location? encounterDisplayName = null)
    {
        this.SetJobId();
        _ = this.loader.Load(encounterDisplayName);
    }

    public void Disable()
    {
        this.loader.Cancel();
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
