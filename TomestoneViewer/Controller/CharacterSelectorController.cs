using System;
using System.Linq;

using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.GameSystems;

namespace TomestoneViewer.Controller;

public class CharacterSelectorController(
    CharDataManager charDataManager,
    TerritoryOfInterestDetector territoryOfInterestDetector)
{
    private readonly CharDataManager charDataManager = charDataManager;
    private readonly TerritoryOfInterestDetector territoryOfInterestDetector = territoryOfInterestDetector;

    public void RefreshPartyList()
    {
        this.UpdatePartyMembers();
        this.charDataManager.FetchPartyLogs();
    }

    public void RefreshPartyData(Location? location = null)
    {
        this.UpdatePartyMembers();
        this.charDataManager.SetTerritoryId(location?.TerritoryId ?? this.territoryOfInterestDetector.TerritoryId);
        this.charDataManager.FetchPartyLogs();
    }

    public void Select(CharSelector selector)
    {
        this.charDataManager.SetCharacter(selector);
    }

    private void UpdatePartyMembers()
    {
        this.charDataManager.UpdatePartyMemebers(
            new TeamList()
            .Members
            .Where(teamMember => teamMember.IsInParty)
            .Select(teamMember => (teamMember.CharacterId, teamMember.JobId))
            .ToList());
    }
}
