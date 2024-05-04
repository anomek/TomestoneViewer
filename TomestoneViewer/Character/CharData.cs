using System.Collections.ObjectModel;
using System.Linq;

using Dalamud.Game.ClientState.Objects.SubKinds;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character;

public class CharData(CharacterId characterId)
{
    private readonly CharacterId characterId = characterId;
    private readonly CharDataLoader loader = new(characterId);


    public ReadOnlyDictionary<string, EncounterData> EncounterData => this.loader.EncounterData;

    public IEncounterDataError? CharError => this.loader.LoadError;

    public CharacterId CharId => this.characterId;

    public uint JobId { get; set; } = 0;


    public void FetchLogs(string? encounterDisplayName = null)
    {
        this.SetJobId();
        // no await
        this.loader.Load(encounterDisplayName);
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
                    && playerCharacter.Name.TextValue == CharId.FullName
                    && playerCharacter.HomeWorld.GameData?.Name.RawString == CharId.World)
                {
                    JobId = playerCharacter.ClassJob.Id;
                    return;
                }
            }
        }

        // if not in object table, search in the team list (can give 0 if normal party member in another zone)
        Service.TeamManager.UpdateTeamList();
        var member = Service.TeamManager.TeamList.FirstOrDefault(member => CharacterId.From(member) == CharId);

        if (member != null)
        {
            JobId = member.JobId;
            return;
        }

        JobId = 0; // avoid stale job id if the current one is not retrievable
    }
}
