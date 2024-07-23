using System;

using Dalamud.Game.Command;
using TomestoneViewer.Character;
using TomestoneViewer.Controller;

namespace TomestoneViewer;

public class Commands : IDisposable
{
    private const string CommandName = "/ts";
    private const string SettingsCommandName = "/tsconfig";

    private readonly MainWindowController mainWindowController;

    public Commands(MainWindowController mainWindowController)
    {
        this.mainWindowController = mainWindowController;

        Service.CommandManager.AddHandler(SettingsCommandName, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Toggle the config window.",
            ShowInHelp = true,
        });

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Toggle the main window.\n" +
                  "         If given \"party\" or \"p\" as argument, open the party view and refresh the party state.\n" +
                  "         If given anything else, open the single view and search for a character name.\n" +
                  "         Support all player character placeholders (<t>, <1>, <mo>, etc.).",
            ShowInHelp = true,
        });
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(CommandName);
        Service.CommandManager.RemoveHandler(SettingsCommandName);
    }

    private void OnCommand(string command, string args)
    {
        var trimmedArgs = args.Trim();
        switch (command)
        {
            case CommandName when trimmedArgs.Equals("config", StringComparison.OrdinalIgnoreCase):
            case SettingsCommandName:
                Service.ConfigWindow.Toggle();
                break;
            case CommandName when string.IsNullOrEmpty(trimmedArgs):
                this.mainWindowController.TogglePartyView();
                break;
            case CommandName:
                if (trimmedArgs.Equals("p", StringComparison.OrdinalIgnoreCase)
                    || trimmedArgs.Equals("party", StringComparison.OrdinalIgnoreCase))
                {
                    this.mainWindowController.OpenParty();
                }
                else
                {
                    this.mainWindowController.OpenCharacter(CharSelector.SelectByName(trimmedArgs));
                }

                break;
        }
    }
}
