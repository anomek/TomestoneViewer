using System;
using System.Linq;

using Dalamud.Game.Gui.ContextMenu;
using Lumina.Excel.Sheets;
using TomestoneViewer.Character;
using TomestoneViewer.Controller;

namespace TomestoneViewer;

public class ContextMenu(MainWindowController mainWindowController, Configuration config)
{
    private readonly MainWindowController mainWindowController = mainWindowController;
    private readonly Configuration config = config;

    public void Enable()
    {
        Service.ContextMenu.OnMenuOpened += this.OnOpenContextMenu;
    }

    public void Disable()
    {
        Service.ContextMenu.OnMenuOpened -= this.OnOpenContextMenu;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "switch looks better")]
    private bool IsMenuValid(IMenuArgs menuOpenedArgs)
    {
        if (this.config.StreamerMode)
        {
            return false;
        }

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
                     && (Service.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == menuTargetDefault.TargetHomeWorld.RowId).IsPublic ?? false);

            case "BlackList":
                return menuTargetDefault.TargetName != string.Empty;
            default:
                return false;
        }
    }

    private void SearchPlayerFromMenu(IMenuArgs menuArgs)
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
                var world = Service.GameData.GetWorldName((ushort)menuTargetDefault.TargetHomeWorld.RowId);
                if (world == null)
                {
                    return;
                }

                this.mainWindowController.OpenCharacter(CharSelector.SelectById(CharacterId.FromFullName(menuTargetDefault.TargetName, world)));
            }
        }
    }

    private void OnOpenContextMenu(IMenuOpenedArgs menuOpenedArgs)
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

    private void Search(IMenuItemClickedArgs menuItemClickedArgs)
    {
        if (!IsMenuValid(menuItemClickedArgs))
        {
            return;
        }

        this.SearchPlayerFromMenu(menuItemClickedArgs);
    }

    private static unsafe string GetBlacklistSelectFullName()
    {
        // FIXME: implement this one day
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
