using System;
using System.Linq;

using TomestoneViewer.Character;
using TomestoneViewer.GameSystems;

namespace TomestoneViewer.Controller;

public class CharacterSelectorController(CharDataManager charDataManager)
{
    private readonly CharDataManager charDataManager = charDataManager;

    public void RefreshPartyList()
    {
        this.UpdatePartyMembers();
        this.charDataManager.FetchPartyLogs();
    }

    public void RefreshPartyData()
    {
        this.UpdatePartyMembers();
        this.charDataManager.SetTerritoryId(Service.PartyFinderDetector.TerritoryId);
        this.charDataManager.FetchPartyLogs();
    }

    public void Select(CharSelector selector)
    {
        this.charDataManager.SetCharacter(selector);
    }

    private void UpdatePartyMembers()
    {
        var previousPartyList = this.charDataManager.PartyMembers;

        var newPartyList = new TeamList()
            .Members
            .Where(teamMember => teamMember.IsInParty)
            .Select(teamMember =>
            {
                var partyMemberId = teamMember.CharacterId;
                var charData = previousPartyList.FirstOrDefault(x => x.CharId == partyMemberId) ?? new CharData(partyMemberId);
                charData.JobId = teamMember.JobId;
                return charData;
            })
            .ToList();

        var characterIds = newPartyList.Select(charData => charData.CharId).ToHashSet();
        foreach (var charData in previousPartyList
            .Where(previousCharData => !characterIds.Contains(previousCharData.CharId)))
        {
            charData.Disable();
        }

        this.charDataManager.UpdatePartyMemebers(newPartyList);
    }
}
