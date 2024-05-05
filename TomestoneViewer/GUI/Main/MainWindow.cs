using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace TomestoneViewer.GUI.Main;

public class MainWindow : Window
{
    private readonly HeaderBar headerBar = new();
    private readonly Table table = new();

    private bool partyView;

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
        Service.CharDataManager.SetEncounter(Service.PartyFinderDetector.TerritoryId);
    }

    public override void OnClose()
    {
        this.Flags &= ~ImGuiWindowFlags.NoFocusOnAppearing;
    }

    public override void Draw()
    {
        MenuBar.Draw(this.partyView);

        if (!this.partyView)
        {
            this.headerBar.Draw();
        }

        this.table.Draw(this.partyView);
    }

    public void SetPartyView(bool v)
    {
        if (this.partyView != v)
        {
            this.TogglePartyView();
        }
    }

    public void TogglePartyView()
    {
        this.partyView = !this.partyView;
        if (this.partyView)
        {
            Service.CharDataManager.SetEncounter(Service.PartyFinderDetector.TerritoryId);
            Service.CharDataManager.UpdatePartyMembers();
        }
    }
}
