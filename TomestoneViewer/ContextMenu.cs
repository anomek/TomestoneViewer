using System.Linq;

using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using TomestoneViewer.Character;
using TomestoneViewer.Controller;

namespace TomestoneViewer;

public class ContextMenu(MainWindowController mainWindowController)
{
    private readonly MainWindowController mainWindowController = mainWindowController;

    public void Enable()
    {
        Service.ContextMenu.OnMenuOpened += this.OnOpenContextMenu;
    }

    public void Disable()
    {
        Service.ContextMenu.OnMenuOpened -= this.OnOpenContextMenu;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "switch looks better")]
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
            default:
                return false;
        }
    }

    private void SearchPlayerFromMenu(MenuArgs menuArgs)
    {
        if (menuArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }

        if (IsPartyAddon(menuArgs.AddonName))
        {
            this.mainWindowController.OpenParty();
        }
        else
        {
            if (menuArgs.AddonName == "BlackList")
            {
                this.mainWindowController.OpenCharacter(CharSelector.SelectByName(GetBlacklistSelectFullName()));
            }
            else
            {
                var world = Service.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == menuTargetDefault.TargetHomeWorld.Id);
                if (world is not { IsPublic: true })
                {
                    return;
                }

                this.mainWindowController.OpenCharacter(CharSelector.SelectByName(menuTargetDefault.TargetName, world.Name));
            }
        }
    }

    private void OnOpenContextMenu(MenuOpenedArgs menuOpenedArgs)
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
            OnClicked = this.Search,
        });
    }

    private void Search(MenuItemClickedArgs menuItemClickedArgs)
    {
        if (!IsMenuValid(menuItemClickedArgs))
        {
            return;
        }

        this.SearchPlayerFromMenu(menuItemClickedArgs);
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
