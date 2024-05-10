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
            var partyLeaderAddon = StripNonAsci(addon->PartyLeaderTextNode->NodeText.ToString());
            var partyLeaderReal = new TeamList().Leader?.CharacterId?.FullName;
            if (partyLeaderAddon != partyLeaderReal)
            {
                Service.PluginLog.Info("In party, ignoring addon close");
                return;
            }
            else
            {
                Service.PluginLog.Info("Looking at your own party, update territory");
            }
        }

        var dutyName = StripNonAsci(addon->DutyNameTextNode->NodeText.ToString());

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

    private static string StripNonAsci(string input)
    {
        while (input.Length > 5 && input[0] == 2)
        {
            input = input[5..];
        }

        return input;
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
