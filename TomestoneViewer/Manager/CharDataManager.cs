using System;
using System.Collections.Generic;
using System.Linq;
using TomestoneViewer.Model;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;

namespace TomestoneViewer.Manager;

public class CharDataManager
{
    private string? currentEncounterDisplayName;

    public SearchCharacterId SearchCharacterId { get; private set; } = new();

    public CharData? DisplayedChar { get; private set; }

    public CharacterError? CharacterError { get; private set; }

    public List<CharData> PartyMembers { get; private set; } = [];


    public string? CurrentEncounterDisplayName
    {
        get => this.currentEncounterDisplayName;
        set
        {
            this.currentEncounterDisplayName = value;
            this.FetchPartyLogs();
        }
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
                this.PartyMembers.Add(charData);
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
            _ = charData.Disable();
        }

        this.PartyMembers.RemoveAll(x => !currPartyMembers.Any(y => x.CharId == CharacterId.From(y)));

        this.PartyMembers = [.. this.PartyMembers.OrderBy(
            charData => currPartyMembers.FindIndex(
                member => charData.CharId == CharacterId.From(member)))];

        this.FetchPartyLogs();
    }

    public void SetCharacter(CharSelector selector)
    {
        if (selector.CharError != null)
        {
            this.CharacterError = selector.CharError.Value;
            this.DisplayedChar = null;
        }
        else if (selector.CharId != null)
        {
            var previousDisplayedChar = this.DisplayedChar;
            var newCharacterData = new CharData(selector.CharId);
            this.DisplayedChar = newCharacterData;
            if (previousDisplayedChar != null)
            {
                previousDisplayedChar?.Disable()
                     .ContinueWith(x => newCharacterData.FetchLogs());
            }
            else
            {
                this.DisplayedChar.FetchLogs();
            }

            this.CharacterError = null;
            this.SearchCharacterId.Copy(newCharacterData.CharId);
        }
    }

    public void Reset()
    {
        this.PartyMembers.Clear();
        this.DisplayedChar = null;
        this.SearchCharacterId.Reset();
    }

    private void FetchPartyLogs()
    {
        if (this.CurrentEncounterDisplayName != null)
        {
            foreach (var charData in this.PartyMembers)
            {
                charData.FetchLogs(this.CurrentEncounterDisplayName);
            }
        }
    }
}
