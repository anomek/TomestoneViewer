using System.Linq;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using TomestoneViewer.Character;

namespace TomestoneViewer;

public class ContextMenu
{
    public static void Enable()
    {
        Service.ContextMenu.OnMenuOpened += OnOpenContextMenu;
    }

    public static void Disable()
    {
        Service.ContextMenu.OnMenuOpened -= OnOpenContextMenu;
    }

    private static bool IsMenuValid(MenuArgs menuOpenedArgs)
    {
        if (menuOpenedArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return false;
        }

        switch (menuOpenedArgs.AddonName)
        {
            case null: // Nameplate/Model menu
            case "LookingForGroup":
            case "PartyMemberList":
            case "FriendList":
            case "FreeCompany":
            case "SocialList":
            case "ContactList":
            case "ChatLog":
            case "LinkShell":
            case "CrossWorldLinkshell":
            case "ContentMemberList": // Eureka/Bozja/...
            case "BeginnerChatList":
            case "_PartyList":
                return menuTargetDefault.TargetName != string.Empty
                     && (Service.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == menuTargetDefault.TargetHomeWorld.Id)?.IsPublic ?? false);

            case "BlackList":
                return menuTargetDefault.TargetName != string.Empty;
        }

        return false;
    }

    private static void SearchPlayerFromMenu(MenuArgs menuArgs)
    {
        if (menuArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }

        if (IsPartyAddon(menuArgs.AddonName))
        {
            Service.MainWindow.IsPartyView = true;
            Service.CharDataManager.UpdatePartyMembers();
        }
        else
        {
            if (menuArgs.AddonName == "BlackList")
            {
                Service.CharDataManager.SetCharacter(CharSelector.SelectByName(GetBlacklistSelectFullName()));
            }
            else
            {
                var world = Service.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == menuTargetDefault.TargetHomeWorld.Id);
                if (world is not { IsPublic: true })
                {
                    return;
                }

                Service.CharDataManager.SetCharacter(CharSelector.SelectByName(menuTargetDefault.TargetName, world.Name));
            }

            Service.MainWindow.IsPartyView = false;
        }

        Service.MainWindow.Open();
    }

    private static void OnOpenContextMenu(MenuOpenedArgs menuOpenedArgs)
    {
        if (!Service.Interface.UiBuilder.ShouldModifyUi || !IsMenuValid(menuOpenedArgs))
        {
            return;
        }

        menuOpenedArgs.AddMenuItem(new MenuItem
        {
            PrefixChar = 'T',
            PrefixColor = 24,
            Name = menuOpenedArgs.AddonName == "_PartyList"
                    ? "Party Tomestone Summary"
                    : "Search Tomestone",
            OnClicked = Search,
        });
    }

    private static void Search(MenuItemClickedArgs menuItemClickedArgs)
    {
        if (!IsMenuValid(menuItemClickedArgs))
        {
            return;
        }

        SearchPlayerFromMenu(menuItemClickedArgs);
    }

    private static unsafe string GetBlacklistSelectFullName()
    {
        var agentBlackList = (AgentBlacklist*)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.SocialBlacklist);
        if (agentBlackList != null)
        {
            return MemoryHelper.ReadSeString(&agentBlackList->SelectedPlayerFullName).TextValue;
        }

        return string.Empty;
    }

    private static bool IsPartyAddon(string? menuArgsAddonName)
    {
        return menuArgsAddonName switch
        {
            "PartyMemberList" => true,
            "_PartyList" => true,
            _ => false,
        };
    }
}