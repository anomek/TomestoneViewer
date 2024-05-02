using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace TomestoneViewer.GUI.Main;

public class MainWindow : Window
{
    public bool IsPartyView;

    private readonly HeaderBar headerBar = new();
    private readonly Table table = new();

    public MainWindow()
        : base("Tomestone Viewer##TomestoneViewerMainWindow10")
    {
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
        MenuBar.Draw();

        if (!this.IsPartyView)
        {
            this.headerBar.Draw();
        }

        this.table.Draw();
    }
}
