using System.Collections.Generic;
using System.Numerics;

using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using TomestoneViewer.Character;
using TomestoneViewer.Controller;

namespace TomestoneViewer.GUI.Main;

public class MainWindow : Window
{
    private readonly MenuBar menuBar;
    private readonly HeaderBar headerBar;
    private readonly Table table;

    public bool PartyView { get; set; } = true;

    public MainWindow(
        IReadOnlyList<CharData> partyList,
        MainWindowController mainWindowController)
        : base("Tomestone Viewer##TomestoneViewerMainWindow")
    {
        this.menuBar = new MenuBar(mainWindowController);
        this.headerBar = new HeaderBar(partyList);
        this.table = new Table(mainWindowController);

        this.RespectCloseHotkey = true;
        this.Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar;
    }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGui.SetNextWindowSize(new Vector2(HeaderBar.GetMinWindowSize(), -1));
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
        this.menuBar.Draw(this.PartyView);

        if (!this.PartyView)
        {
            this.headerBar.Draw();
        }

        this.table.Draw(this.PartyView);
    }
}
