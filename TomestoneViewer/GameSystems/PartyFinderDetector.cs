using System;
using System.Linq;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace TomestoneViewer.GameSystems;

public unsafe class PartyFinderDetector
{
    private readonly AddonInterface<AddonLookingForGroupDetail> partyFinderDetailsAddon;
    private readonly AddonInterface<AddonLookingForGroupCondition> createPartyFinderAddon;

    public uint TerritoryId { get; private set; }

    public PartyFinderDetector()
    {
        this.partyFinderDetailsAddon = new("LookingForGroupDetail");
        this.partyFinderDetailsAddon.AddOnEndHandler(this.OnLookingForGroupDetailClose);

        this.createPartyFinderAddon = new("LookingForGroupCondition");
        this.createPartyFinderAddon.AddOnEndHandler(this.OnLookingForGroupConditionClose);
    }

    public void Dispose()
    {
        this.partyFinderDetailsAddon.Dispose();
        this.createPartyFinderAddon.Dispose();
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

    private void UpdateTerritoryId(uint? territoryId)
    {
        if (territoryId != null)
        {
            // Left commented log as a useful tool for detecting territoryId in future
            // Service.PluginLog.Info($"Closed dialog for territoryId={targetTerritoryId}");
            this.TerritoryId = territoryId.Value;
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
