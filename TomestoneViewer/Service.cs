using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using TomestoneViewer.Character;
using TomestoneViewer.Character.TomestoneClient;
using TomestoneViewer.GameSystems;
using TomestoneViewer.GUI.Config;
using TomestoneViewer.GUI.Main;
using TomestoneViewer.Manager;

#pragma warning disable SA1134 // AttributesMustNotShareLine
namespace TomestoneViewer;

internal class Service
{
    internal static Configuration Configuration { get; set; } = null!;

    internal static Commands Commands { get; set; } = null!;

    internal static ConfigWindow ConfigWindow { get; set; } = null!;

    internal static MainWindow MainWindow { get; set; } = null!;

    internal static CharDataManager CharDataManager { get; set; } = null!;

    internal static GameDataManager GameDataManager { get; set; } = null!;

    internal static HistoryManager HistoryManager { get; set; } = null!;

    [PluginService] internal static DalamudPluginInterface Interface { get; private set; } = null!;

    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;

    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService] internal static ICondition Condition { get; private set; } = null!;

    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService] internal static IFlyTextGui FlyTextGui { get; private set; } = null!;

    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;

    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;

    [PluginService] internal static IKeyState KeyState { get; private set; } = null!;

    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;

    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;

    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;

    [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;

    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    [PluginService] internal static IAddonEventManager AddonEventManager { get; private set; } = null!;
}
