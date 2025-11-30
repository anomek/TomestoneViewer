using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Memory.Exceptions;
using Dalamud.Plugin;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GameSystems;
using TomestoneViewer.GUI.Config;
using TomestoneViewer.GUI.Main;
using TomestoneViewer.Manager;

namespace TomestoneViewer;

// ReSharper disable once UnusedType.Global
public sealed class TomestoneViewerPlugin : IDalamudPlugin
{
    private readonly WindowSystem windowSystem;

    private readonly ContextMenu contextMenu;
    private readonly TerritoryOfInterestDetector territorryOfInterestDetector;

    public TomestoneViewerPlugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();



        this.territorryOfInterestDetector = new TerritoryOfInterestDetector(Location.AllTerritories());

        var tomestoneClient = new CachedTomestoneClient(new SyncTomestoneClient(new SafeTomestoneClient(new WebTomestoneClient())));
        var cachePath = pluginInterface.ConfigDirectory + "/FFLogsOldEncountersCache";
        var lowLevelFFLogsClient = new LowLevelFFLogsClient();
        var fflogsClient = new ToggleableFFLogsClient(() => Service.Configuration.FFLogsEnabled, new CachedFFLogsClient(cachePath, new SyncFFLogsClient(new SafeFFLogsClient(new WebFFLogsClient(lowLevelFFLogsClient)))));

        Service.CharDataManager = new CharDataManager(new CharDataFactory(tomestoneClient, fflogsClient));

        var windowsController = new WindowsController(new CharacterSelectorController(Service.CharDataManager, this.territorryOfInterestDetector));

        Service.Commands = new Commands(windowsController);

        Service.GameData = new GameData();
        Service.HistoryManager = new HistoryManager();
        Service.Fonts = new GUI.Fonts(() => Service.Configuration.UseDefaultFont);

        Service.MainWindow = new MainWindow(Service.CharDataManager.PartyMembers, windowsController, Service.Configuration, lowLevelFFLogsClient);
        Service.ConfigWindow = new ConfigWindow(
            Service.Configuration,
            lowLevelFFLogsClient,
            new(pluginInterface.GetPluginConfigDirectory()));
        windowsController.RegisterWindows(Service.MainWindow, Service.ConfigWindow);

        this.windowSystem = new WindowSystem("Tomestone Viewer");
        this.windowSystem.AddWindow(Service.ConfigWindow);
        this.windowSystem.AddWindow(Service.MainWindow);

        this.contextMenu = new ContextMenu(windowsController, Service.Configuration);
        this.contextMenu.Enable();

        Service.Interface.UiBuilder.OpenMainUi += OpenMainUi;
        Service.Interface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;


#if DEBUG
        Service.Interface.OpenDeveloperMenu();
#endif
    }

    public void Dispose()
    {
        Service.Commands.Dispose();
        Service.Fonts.Dispose();

        this.contextMenu.Disable();
        this.territorryOfInterestDetector.Dispose();

        Service.Interface.UiBuilder.OpenMainUi -= OpenMainUi;
        Service.Interface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;
    }

    private static void OpenMainUi()
    {
        Service.MainWindow.IsOpen = true;
    }

    private static void OpenConfigUi()
    {
        Service.ConfigWindow.IsOpen = true;
    }
}
