using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using TomestoneViewer.GUI.Config;
using TomestoneViewer.GUI.Main;
using TomestoneViewer.Manager;

namespace TomestoneViewer;

// ReSharper disable once UnusedType.Global
public sealed class TomestoneViewerPlugin : IDalamudPlugin
{
    private readonly WindowSystem windowSystem;

    public TomestoneViewerPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Service.Commands = new Commands();
        Service.CharDataManager = new CharDataManager();
        Service.GameDataManager = new GameDataManager();
        Service.OpenWithManager = new OpenWithManager();
        Service.HistoryManager = new HistoryManager();
        Service.TeamManager = new TeamManager();
        Service.TomestoneClient = new TomestoneClient();

        Service.MainWindow = new MainWindow();
        Service.ConfigWindow = new ConfigWindow();
        this.windowSystem = new WindowSystem("Tomestone Viewer");
        this.windowSystem.AddWindow(Service.ConfigWindow);
        this.windowSystem.AddWindow(Service.MainWindow);

        ContextMenu.Enable();

        Service.Interface.UiBuilder.OpenMainUi += OpenMainUi;
        Service.Interface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;
    }

    public void Dispose()
    {
        Commands.Dispose();
        Service.OpenWithManager.Dispose();

        ContextMenu.Disable();

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
