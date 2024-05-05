using System;
using Dalamud.Game.Command;
using TomestoneViewer.Character;

namespace TomestoneViewer;

public class Commands
{
    private const string CommandName = "/ts";
    private const string SettingsCommandName = "/tsconfig";

    public Commands()
    {

        
        Service.CommandManager.AddHandler(SettingsCommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the config window.",
            ShowInHelp = true,
        });

        

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the main window.\n" +
                  "         If given \"party\" or \"p\" as argument, open the party view and refresh the party state.\n" +
                  "         If given anything else, open the single view and search for a character name.\n" +
                  "         Support all player character placeholders (<t>, <1>, <mo>, etc.).",
            ShowInHelp = true,
        });
    }

    public static void Dispose()
    {
        Service.CommandManager.RemoveHandler(CommandName);
        Service.CommandManager.RemoveHandler(SettingsCommandName);
    }

    private static void OnCommand(string command, string args)
    {
        var trimmedArgs = args.Trim();
        switch (command)
        {
            case CommandName when trimmedArgs.Equals("config", StringComparison.OrdinalIgnoreCase):
            case SettingsCommandName:
                Service.ConfigWindow.Toggle();
                break;
            case CommandName when string.IsNullOrEmpty(trimmedArgs):
                Service.MainWindow.Toggle();
                break;
            case CommandName:
                Service.MainWindow.Open();
                if (trimmedArgs.Equals("p", StringComparison.OrdinalIgnoreCase)
                    || trimmedArgs.Equals("party", StringComparison.OrdinalIgnoreCase))
                {
                    Service.MainWindow.SetPartyView(true);
                    Service.CharDataManager.UpdatePartyMembers();
                    break;
                }

                Service.CharDataManager.SetCharacter(CharSelector.SelectByName(trimmedArgs));
                break;
        }
    }
}
