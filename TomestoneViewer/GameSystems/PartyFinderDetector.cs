using System;
using System.Linq;

using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.GeneratedSheets;

namespace TomestoneViewer.GameSystems;

public unsafe class PartyFinderDetector
{
    private readonly AddonInterface<AddonLookingForGroupDetail> partyFinderDetailsAddon;

    public uint TerritoryId { get; private set; }

    public PartyFinderDetector()
    {
        this.partyFinderDetailsAddon = new("LookingForGroupDetail");
        this.partyFinderDetailsAddon.AddOnEndHandler(this.OnAddonCloses);
    }

    public void Dispose()
    {
        this.partyFinderDetailsAddon.Dispose();
    }

    private void OnAddonCloses(AddonLookingForGroupDetail* addon)
    {
        if (IsInParty())
        {
            Service.PluginLog.Info("In party, ignoring addon close");

            // Already in party, looking at party finder doesn't really matter
            // TODO: actualy this could be "Recruitment Criteria" addon in which case we might want to update regionId
            return;
        }

        var dutyName = addon->DutyNameTextNode->NodeText.ToString();

        // cut sprout from duty name
        if (dutyName.Length > 5 && dutyName[0] == 2)
        {
            dutyName = dutyName[5..];
        }

        var targetTerritoryId = Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
             .FirstOrDefault(entry => string.Equals(
                 entry.Name.ToString(),
                 dutyName,
                 StringComparison.OrdinalIgnoreCase))?
                 .TerritoryType.Row;

        if (targetTerritoryId != null)
        {
            // Left commented log as a useful tool for detecting territoryId in future
            // Service.PluginLog.Info($"Closed dialog for territoryId={targetTerritoryId}");
            // Can't be sure that party was joined, but this is a best effort
            this.TerritoryId = targetTerritoryId.Value;
        }
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
