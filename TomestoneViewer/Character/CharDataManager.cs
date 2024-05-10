using System;
using System.Collections.Generic;
using System.Linq;

using Lumina.Excel.GeneratedSheets;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.GameSystems;
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
        set => this.currentEncounter = value;
    }

    public IRenderableError? CharacterError
    {
        get => (IRenderableError?)this.characterSelectorError ?? this.displayedChar?.CharError;
    }

    public void UpdatePartyMemebers(List<CharData> partyMembers)
    {
        this.partyMembers.Clear();
        this.partyMembers.AddRange(partyMembers);
    }

    public void SetTerritoryId(uint? teritorryId)
    {
        if (teritorryId == null)
        {
            return;
        }

        Service.PluginLog.Info($"Set encounter {teritorryId}");
        var encounter = Location.Find(new TerritoryId(teritorryId.Value));
        if (encounter != null)
        {
            this.CurrentEncounter = encounter;
        }
    }

    public void SetCharacter(CharSelector selector)
    {
        Service.PluginLog.Debug($"CharDataManger.SetCharacter {selector}");
        var previousDisplayedChar = this.DisplayedChar;
        if (selector.Error != null)
        {
            this.characterSelectorError = selector.Error;
            this.displayedChar = null;
        }
        else if (selector.CharId != null)
        {
            this.displayedChar = new CharData(selector.CharId);
            this.displayedChar.FetchLogs();
            this.characterSelectorError = null;
            this.searchCharacterId.Copy(this.displayedChar.CharId);
        }

        previousDisplayedChar?.Disable();
    }

    public void FetchPartyLogs()
    {
        Service.PluginLog.Debug("CharDataManger.FetchPartyLogs");
        foreach (var charData in this.PartyMembers)
        {
            charData.FetchLogs(this.currentEncounter);
        }
    }
}
