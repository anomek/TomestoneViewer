using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using TomestoneViewer.Character;
using TomestoneViewer.Character.TomestoneClient;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Config;
using TomestoneViewer.GUI.Main;
using TomestoneViewer.Manager;

namespace TomestoneViewer;

// ReSharper disable once UnusedType.Global
public sealed class TomestoneViewerPlugin : IDalamudPlugin
{
    private readonly WindowSystem windowSystem;

    private readonly ContextMenu contextMenu;

    public TomestoneViewerPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Service.CharDataManager = new CharDataManager();

        var characterSelectorController = new CharacterSelectorController(Service.CharDataManager);
        var mainWindowController = new MainWindowController(characterSelectorController);

        Service.Commands = new Commands(mainWindowController);

        Service.GameDataManager = new GameDataManager();
        Service.HistoryManager = new HistoryManager();
        Service.TomestoneClient = new CachedTomestoneClient(new SyncTomestoneClient(new SafeTomestoneClient(new WebTomestoneClient())));
        Service.PartyFinderDetector = new GameSystems.PartyFinderDetector();

        Service.MainWindow = new MainWindow(Service.CharDataManager.PartyMembers, characterSelectorController, mainWindowController);
        mainWindowController.RegisterMainWindow(Service.MainWindow);
        Service.ConfigWindow = new ConfigWindow();

        this.windowSystem = new WindowSystem("Tomestone Viewer");
        this.windowSystem.AddWindow(Service.ConfigWindow);
        this.windowSystem.AddWindow(Service.MainWindow);

        this.contextMenu = new ContextMenu(mainWindowController);
        this.contextMenu.Enable();

        Service.Interface.UiBuilder.OpenMainUi += OpenMainUi;
        Service.Interface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;
    }

    public void Dispose()
    {
        Service.Commands.Dispose();
        Service.PartyFinderDetector.Dispose();

        this.contextMenu.Disable();

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
