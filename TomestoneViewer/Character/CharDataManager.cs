using System.Collections.Generic;
using System.Linq;

using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Model;

namespace TomestoneViewer.Character;

public class CharDataManager(CharDataFactory charDataFactory)
{
    private readonly SearchCharacterId searchCharacterId = new();
    private readonly List<CharData> partyMembers = [];
    private readonly CharDataFactory charDataFactory = charDataFactory;

    private Location currentEncounter = Location.All()[0];
    private CharData? displayedChar;
    private CharacterSelectorError? characterSelectorError = CharacterSelectorError.NoCharacterSelected;

    public SearchCharacterId SearchCharacterId => this.searchCharacterId;

    public CharData? DisplayedChar => this.displayedChar;

    public IReadOnlyList<CharData> PartyMembers => this.partyMembers;

    public Location CurrentEncounter
    {
        get => this.currentEncounter;
        private set => this.currentEncounter = value;
    }

    public IRenderableError? CharacterError
    {
        get => (IRenderableError?)this.characterSelectorError ?? this.displayedChar?.CharError;
    }

    public void UpdatePartyMemebers(List<(CharacterId Id, JobId JobId)> partyMembers)
    {
        var newPartyList = partyMembers
            .Select(character =>
            {
                var charData = this.partyMembers.FirstOrDefault(x => x.CharId == character.Id) ?? this.charDataFactory.Create(character.Id);
                charData.JobId = character.JobId;
                return charData;
            })
            .ToList();

        var characterIds = newPartyList.Select(charData => charData.CharId).ToHashSet();
        foreach (var charData in this.partyMembers
            .Where(previousCharData => !characterIds.Contains(previousCharData.CharId)))
        {
            charData.Disable();
        }

        this.partyMembers.Clear();
        this.partyMembers.AddRange(newPartyList);
    }

    public void SetTerritoryId(TerritoryId? teritorryId)
    {
        if (teritorryId == null)
        {
            return;
        }

        Service.PluginLog.Info($"Set encounter {teritorryId}");
        var encounter = Location.Find(teritorryId);
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
            this.displayedChar = this.charDataFactory.Create(selector.CharId);
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
