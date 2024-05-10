using System;
using System.Collections.Generic;
using System.Linq;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.GameSystems;

public unsafe class TerritoryOfInterestDetector
{
    private readonly IReadOnlySet<TerritoryId> validTerritories;

    private readonly AddonInterface<AddonLookingForGroupDetail> partyFinderDetailsAddon;
    private readonly AddonInterface<AddonLookingForGroupCondition> createPartyFinderAddon;

    public TerritoryId? TerritoryId { get; private set; }

    public TerritoryOfInterestDetector(IReadOnlySet<TerritoryId> validTerritories)
    {
        this.validTerritories = validTerritories;

        this.partyFinderDetailsAddon = new("LookingForGroupDetail");
        this.partyFinderDetailsAddon.AddOnEndHandler(this.OnLookingForGroupDetailClose);

        this.createPartyFinderAddon = new("LookingForGroupCondition");
        this.createPartyFinderAddon.AddOnEndHandler(this.OnLookingForGroupConditionClose);

        Service.ClientState.TerritoryChanged += this.OnTerritoryChanged;
    }

    public void Dispose()
    {
        this.partyFinderDetailsAddon.Dispose();
        this.createPartyFinderAddon.Dispose();
        Service.ClientState.TerritoryChanged -= this.OnTerritoryChanged;
    }

    private void OnTerritoryChanged(ushort territoryId)
    {
        this.UpdateTerritoryId(territoryId);
    }

    private void OnLookingForGroupConditionClose(AddonLookingForGroupCondition* addon)
    {
        var dutyName = SelectedItem(addon->DutyNameDropDown);

        // can't be sure that pf was created, but this is good enough
        this.UpdateTerritoryId(FindTerritoryId(dutyName));
    }

    private void OnLookingForGroupDetailClose(AddonLookingForGroupDetail* addon)
    {
        if (IsInParty())
        {
            var partyLeaderAddon = StripNonAsci(addon->PartyLeaderTextNode->NodeText.ToString());
            var partyLeaderReal = new TeamList().Leader?.CharacterId?.FullName;
            if (partyLeaderAddon != partyLeaderReal)
            {
                return;
            }
        }

        var dutyName = StripNonAsci(addon->DutyNameTextNode->NodeText.ToString());

        // Can't be sure that party was joined, but this is a best effort
        this.UpdateTerritoryId(FindTerritoryId(dutyName));
    }

    private void UpdateTerritoryId(uint? territoryIdInt)
    {
        if (territoryIdInt == null)
        {
            return;
        }

        var territoryId = new TerritoryId(territoryIdInt.Value);

        // Left commented log as a useful tool for detecting territoryId in future
        // Service.PluginLog.Info($"Selecting territoryId territoryId={territoryId}");
        if (this.validTerritories.Contains(territoryId))
        {
            this.TerritoryId = territoryId;
        }
    }

    private static uint? FindTerritoryId(string dutyName)
    {
        return Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
               .FirstOrDefault(entry => string.Equals(
                     entry.Name.ToString(),
                     dutyName,
                     StringComparison.OrdinalIgnoreCase))?
               .TerritoryType.Row;
    }

    private static string StripNonAsci(string input)
    {
        while (input.Length > 5 && input[0] == 2)
        {
            input = input[5..];
        }

        return input;
    }

    private static string SelectedItem(AtkComponentDropDownList* dropdown)
    {
        var selectedItem = dropdown->GetSelectedItemIndex();
        return dropdown->List->GetItemRenderer(selectedItem)->AtkComponentButton.ButtonTextNode->NodeText.ToString();
    }

    private static bool IsInParty()
    {
        if (GroupManager.Instance()->MemberCount > 0)
        {
            return true;
        }

        var crossRealm = InfoProxyCrossRealm.Instance();
        if (crossRealm != null)
        {
            return crossRealm->IsInCrossRealmParty > 0;
        }

        return false;
    }
}
