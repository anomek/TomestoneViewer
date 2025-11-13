using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace TomestoneViewer.GUI.Config;

public class ConfigWindow : Window
{
    private readonly Configuration config;

    public ConfigWindow(Configuration config)
        : base("Tomestone Viewer Configuration##TomestoneViewerConfigWindow")
    {
        this.RespectCloseHotkey = true;
        this.Flags = ImGuiWindowFlags.AlwaysAutoResize;

        this.config = config;
    }

    public override void Draw()
    {
        var loadFromFFlogs = this.config.FFLogsEnabled;
        if (ImGui.Checkbox("Load data from FFLogs", ref loadFromFFlogs))
        {
            this.config.FFLogsEnabled = loadFromFFlogs;
            this.config.Save();
        }

        var useDefaultFont = this.config.UseDefaultFont;
        if (ImGui.Checkbox("Use default font", ref useDefaultFont))
        {
            this.config.UseDefaultFont = useDefaultFont;
            this.config.Save();
        }

        var streamerMode = this.config.StreamerMode;
        if (ImGui.Checkbox("Streamer mode", ref streamerMode))
        {
            this.config.StreamerMode = streamerMode;
            this.config.Save();
        }
    }
}
