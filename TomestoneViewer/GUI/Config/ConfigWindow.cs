using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace TomestoneViewer.GUI.Config;

public class ConfigWindow : Window
{
    public ConfigWindow()
        : base("Tomestone Viewer Configuration##TomestoneViewerConfigWindow")
    {
        this.RespectCloseHotkey = true;

        this.Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("No configuration yet, sorry!                       \n\n\n");
    }
}
