using System.Collections.Generic;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Windowing;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.Controller;

using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace TomestoneViewer.GUI.Main;

public class MainWindow : Window
{
    private readonly MenuBar menuBar;
    private readonly HeaderBar headerBar;
    private readonly Table table;

    public bool PartyView { get; set; } = true;

    internal MainWindow(
        IReadOnlyList<CharData> partyList,
        WindowsController mainWindowController,
        Configuration config,
        LowLevelFFLogsClient lowLevelFFLogsClient)
        : base("Tomestone Viewer##TomestoneViewerMainWindow")
    {
        this.menuBar = new MenuBar(mainWindowController);
        this.headerBar = new HeaderBar(partyList);
        this.table = new Table(mainWindowController, () => config.FFLogsEnabled, lowLevelFFLogsClient);

        this.RespectCloseHotkey = true;
        this.Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar;
    }

    public override void PreDraw()
    {
        base.PreDraw();

        // FIXME: real calculations
        ImGui.SetNextWindowSize(new Vector2(HeaderBar.GetMinWindowSize() * 1.3f, -1));
    }

    public void Open(bool takeFocus = true)
    {
        if (!takeFocus)
        {
            this.Flags |= ImGuiWindowFlags.NoFocusOnAppearing;
        }

        this.IsOpen = true;
    }

    public override void OnClose()
    {
        this.Flags &= ~ImGuiWindowFlags.NoFocusOnAppearing;
    }

    public override void Draw()
    {
        Service.Fonts.Default.Push();
        this.menuBar.Draw(this.PartyView);

        if (!this.PartyView)
        {
            this.headerBar.Draw();
        }

        this.table.Draw(this.PartyView);
        Service.Fonts.Default.Pop();
    }
}
