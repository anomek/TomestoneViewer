using System;
using System.Collections.Generic;
using System.Linq;

using Lumina.Excel.GeneratedSheets;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Model;

namespace TomestoneViewer.Character;

public class CharDataManager
{
    private readonly SearchCharacterId searchCharacterId = new();
    private readonly List<CharData> partyMembers = [];

    private Location currentEncounter = Location.All()[0];
    private CharData? displayedChar;
    private CharacterSelectorError? characterSelectorError = CharacterSelectorError.NoCharacterSelected;

    public SearchCharacterId SearchCharacterId => this.searchCharacterId;

    public CharData? DisplayedChar => this.displayedChar;

    public IReadOnlyList<CharData> PartyMembers => this.partyMembers;

    public Location CurrentEncounter
    {
        get => this.currentEncounter;
        set
        {
            this.currentEncounter = value;
            this.FetchPartyLogs();
        }
    }

    public IRenderableError? CharacterError
    {
        get => (IRenderableError?)this.characterSelectorError ?? this.displayedChar?.CharError;
    }

    public void UpdatePartyMembers()
    {
        Service.TeamManager.UpdateTeamList();
        var currPartyMembers = Service.TeamManager.TeamList.Where(teamMember => teamMember.IsInParty).ToList();

        foreach (var partyMember in currPartyMembers)
        {
            var partyMemberId = CharacterId.From(partyMember);
            var member = this.PartyMembers.FirstOrDefault(x => x.CharId == partyMemberId);

            if (member == null)
            {
                // add new member
                var charData = new CharData(partyMemberId);

                charData.JobId = partyMember.JobId;
                this.partyMembers.Add(charData);
            }
            else
            {
                // update existing member
                member.JobId = partyMember.JobId;
            }
        }

        // remove members that are no longer in party
        foreach (var charData in this.PartyMembers.Where(x => !currPartyMembers.Any(y => x.CharId == CharacterId.From(y))))
        {
            charData.Disable();
        }

        this.partyMembers.RemoveAll(x => !currPartyMembers.Any(y => x.CharId == CharacterId.From(y)));
        this.partyMembers.Sort(CompareByIndex(currPartyMembers));

        this.FetchPartyLogs();
    }

    public void SetEncounter(uint? teritorryId)
    {
        if (teritorryId == null)
        {
            return;
        }

        Service.PluginLog.Info($"Set encounter {teritorryId}");
        var encounter = Location.Find(new TerritoryId(teritorryId.Value));
        if (encounter != null)
        {
            this.currentEncounter = encounter;
        }
    }

    public void SetCharacter(CharSelector selector)
    {
        Service.PluginLog.Debug($"CharDataManger.SetCharacter {selector}");
        if (selector.Error != null)
        {
            this.characterSelectorError = selector.Error;
            this.displayedChar = null;
        }
        else if (selector.CharId != null)
        {
            var previousDisplayedChar = this.DisplayedChar;
            this.displayedChar = new CharData(selector.CharId);
            if (previousDisplayedChar != null)
            {
                previousDisplayedChar?.Disable();
            }

            this.displayedChar.FetchLogs();
            this.characterSelectorError = null;
            this.searchCharacterId.Copy(this.displayedChar.CharId);
        }
    }

    public void Reset()
    {
        Service.PluginLog.Debug("CharDataManger.Reset");
        this.partyMembers.Clear();
        this.displayedChar = null;
        this.characterSelectorError = CharacterSelectorError.NoCharacterSelected;
        this.SearchCharacterId.Reset();
    }

    private static Comparer<CharData> CompareByIndex(List<TeamMember> teamMembers)
    {
        Dictionary<CharacterId, int> dict = [];
        for (var i = 0; i < teamMembers.Count; i++)
        {
            dict[CharacterId.From(teamMembers[i])] = i;
        }

        return Comparer<CharData>.Create((CharData left, CharData right) => dict[left.CharId].CompareTo(dict[right.CharId]));
    }

    private void FetchPartyLogs()
    {
        Service.PluginLog.Debug("CharDataManger.FetchPartyLogs");
        foreach (var charData in this.PartyMembers)
        {
            charData.FetchLogs(this.currentEncounter);
        }
    }
}
