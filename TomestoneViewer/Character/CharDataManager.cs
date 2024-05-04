using System;
using System.Collections.Generic;
using System.Linq;

using Lumina.Excel.GeneratedSheets;
using TomestoneViewer.Model;

namespace TomestoneViewer.Character;

public class CharDataManager
{
    private readonly SearchCharacterId searchCharacterId = new();
    private readonly List<CharData> partyMembers = [];

    private string? currentEncounterDisplayName;
    private CharData? displayedChar;
    private CharacterSelectorError? characterSelectorError = CharacterSelectorError.NoCharacterSelected;

    public SearchCharacterId SearchCharacterId { get => this.searchCharacterId; }

    public CharData? DisplayedChar { get => this.displayedChar; }

    public IReadOnlyList<CharData> PartyMembers { get => this.partyMembers; }


    public string? CurrentEncounterDisplayName
    {
        get => this.currentEncounterDisplayName;
        set
        {
            this.currentEncounterDisplayName = value;
            this.FetchPartyLogs();
        }
    }

    public IRenderableError? CharacterError
    {
        get => (IRenderableError?)this.characterSelectorError ?? this.displayedChar?.CharError;
    }

    public string[] ValidWorlds { get; private set; }

    public CharDataManager()
    {
        var worlds = Service.DataManager.GetExcelSheet<World>()?.Where(world => world.IsPublic && world.DataCenter?.Value?.Region != 0);
        if (worlds == null)
        {
            throw new InvalidOperationException("Sheets weren't ready.");
        }

        this.ValidWorlds = worlds.Select(world => world.Name.RawString).ToArray();
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
        foreach (var charData in PartyMembers.Where(x => !currPartyMembers.Any(y => x.CharId == CharacterId.From(y))))
        {
            charData.Disable();
        }

        this.partyMembers.RemoveAll(x => !currPartyMembers.Any(y => x.CharId == CharacterId.From(y)));

        //      FIXME: sort party member
        //       this.partyMembers.Sort(  = [.. PartyMembers.OrderBy(
        //            charData => currPartyMembers.FindIndex(
        //                member => charData.CharId == CharacterId.From(member)))];

        this.FetchPartyLogs();
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

    private void FetchPartyLogs()
    {
        Service.PluginLog.Debug("CharDataManger.FetchPartyLogs");
        if (this.CurrentEncounterDisplayName != null)
        {
            foreach (var charData in this.PartyMembers)
            {
                charData.FetchLogs(this.CurrentEncounterDisplayName);
            }
        }
    }
}