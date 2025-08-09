using System.Collections.Generic;
using System.Linq;

using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using TomestoneViewer.Character;

namespace TomestoneViewer.GameSystems;

public class TeamList
{
    public List<TeamMember> Members { get; } = [];

    public bool NonEmpty => this.Members.Count > 0;

    public TeamMember? Leader => this.Members.Where(member => member.IsLeader).FirstOrDefault();

    public unsafe TeamList()
    {
        var groupManager = GroupManager.Instance()->MainGroup;
        if (groupManager.MemberCount > 0)
        {
            this.AddMembersFromGroupManager(groupManager);
        }
        else
        {
            var cwProxy = InfoProxyCrossRealm.Instance();
            if (cwProxy->IsInCrossRealmParty)
            {
                var localIndex = cwProxy->LocalPlayerGroupIndex;
                this.AddMembersFromCRGroup(cwProxy->CrossRealmGroups[localIndex], true);

                for (var i = 0; i < cwProxy->CrossRealmGroups.Length; i++)
                {
                    if (i == localIndex)
                    {
                        continue;
                    }

                    this.AddMembersFromCRGroup(cwProxy->CrossRealmGroups[i]);
                }
            }
        }

        // Add self if not in party
        if (this.Members.Count == 0 && Service.ClientState.LocalPlayer != null)
        {
            var selfName = Service.ClientState.LocalPlayer.Name.TextValue;
            var selfWorldId = Service.ClientState.LocalPlayer.HomeWorld.RowId;
            var selfJobId = Service.ClientState.LocalPlayer.ClassJob.RowId;
            this.AddTeamMember(selfName, (ushort)selfWorldId, selfJobId, true, false);
        }
    }

    private unsafe void AddMembersFromCRGroup(CrossRealmGroup crossRealmGroup, bool isLocalPlayerGroup = false)
    {
        for (var i = 0; i < crossRealmGroup.GroupMemberCount; i++)
        {
            var groupMember = crossRealmGroup.GroupMembers[i];
            this.AddTeamMember(groupMember.NameString, (ushort)groupMember.HomeWorld, groupMember.ClassJobId, isLocalPlayerGroup, groupMember.IsPartyLeader);
        }
    }

    private unsafe void AddMembersFromGroupManager(GroupManager.Group groupManager)
    {
        var partyMemberList = AgentModule.Instance()->GetAgentHUD()->PartyMembers;
        var groupManagerIndexLeft = Enumerable.Range(0, groupManager.MemberCount).ToList();

        for (var i = 0; i < groupManager.MemberCount; i++)
        {
            var hudPartyMember = partyMemberList[i];
            var hudPartyMemberNameRaw = hudPartyMember.Name;
            if (hudPartyMemberNameRaw != null)
            {
                var hudPartyMemberName = Util.ReadSeString(hudPartyMemberNameRaw).TextValue;
                for (var j = 0; j < groupManager.MemberCount; j++)
                {
                    // handle duplicate names from different worlds
                    if (!groupManagerIndexLeft.Contains(j))
                    {
                        continue;
                    }

                    var partyMember = groupManager.GetPartyMemberByIndex(j);
                    if (partyMember != null)
                    {
                        var partyMemberName = partyMember->NameString;
                        if (hudPartyMemberName.Equals(partyMemberName))
                        {
                            this.AddTeamMember(partyMemberName, partyMember->HomeWorld, partyMember->ClassJob, true, false);
                            groupManagerIndexLeft.Remove(j);
                            break;
                        }
                    }
                }
            }
        }

        for (var i = 0; i < 20; i++)
        {
            var allianceMember = groupManager.GetAllianceMemberByIndex(i);
            if (allianceMember != null)
            {
                this.AddTeamMember(allianceMember->NameString, allianceMember->HomeWorld, allianceMember->ClassJob, false, false);
            }
        }
    }

    private void AddTeamMember(string fullName, ushort worldId, uint jobId, bool isInParty, bool isLeader)
    {
        var world = Service.GameData.GetWorldName(worldId);
        if (world == null)
        {
            return;
        }

        if (fullName == string.Empty)
        {
            return;
        }

        var splitName = fullName.Split(' ');
        if (splitName.Length != 2)
        {
            return;
        }

        this.Members.Add(new TeamMember(new CharacterId(splitName[0], splitName[1], world), new JobId(jobId), isInParty, isLeader));
    }
}
