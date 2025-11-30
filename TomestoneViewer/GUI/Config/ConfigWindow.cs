using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;
using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.External;

namespace TomestoneViewer.GUI.Config;

public class ConfigWindow : Window
{
    private readonly Configuration config;
    private readonly LowLevelFFLogsClient lowLevelFFLogsClient;
    private readonly FFLogsViewerConfigReader ffLogsViewerConfigReader;

    internal ConfigWindow(
        Configuration config,
        LowLevelFFLogsClient lowLevelFFLogsClient,
        FFLogsViewerConfigReader ffLogsViewerConfigReader)
        : base("Tomestone Viewer Configuration##TomestoneViewerConfigWindow")
    {
        this.RespectCloseHotkey = true;
        this.Flags = ImGuiWindowFlags.AlwaysAutoResize;

        this.config = config;
        this.lowLevelFFLogsClient = lowLevelFFLogsClient;
        this.ffLogsViewerConfigReader = ffLogsViewerConfigReader;
    }

    public override void OnOpen()
    {
        this.ffLogsViewerConfigReader.Refresh();
        var _ = this.lowLevelFFLogsClient.RefreshToken();
    }

    public override void Draw()
    {


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

        ImGui.Separator();

        var loadFromFFlogs = this.config.FFLogsEnabled;
        if (ImGui.Checkbox("Use FF Logs in additon to Tomestone", ref loadFromFFlogs))
        {
            this.config.FFLogsEnabled = loadFromFFlogs;
            this.config.Save();
        }

        if (this.config.FFLogsEnabled)
        {
            var fflogsClientId = this.config.FFLogsClientId;

            if (ImGui.InputText("Client ID", ref fflogsClientId))
            {
                this.config.FFLogsClientId = fflogsClientId;
                this.config.Save();
                var _ = lowLevelFFLogsClient.RefreshToken();
            }

            var fflogsClientSecret = this.config.FFLogsClientSecret;
            if (ImGui.InputText("Client Secret", ref fflogsClientSecret))
            {
                this.config.FFLogsClientSecret = fflogsClientSecret;
                this.config.Save();
                var _ = lowLevelFFLogsClient.RefreshToken();
            }

            if (this.lowLevelFFLogsClient.CredentialsValid)
            {
                ImGui.TextColored(new Vector4(0, 1, 0, 1), "Credentials are valid");
            }
            else
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Credentials are invalid or www.fflogs.com is down\n");
            }

            if (this.ffLogsViewerConfigReader.HasFFlogs)
            {
                if (ImGui.Button("Copy Client ID and Secret from FFLogsViewer plugin"))
                {
                    this.config.FFLogsClientId = this.ffLogsViewerConfigReader.ClientId ?? string.Empty;
                    this.config.FFLogsClientSecret = this.ffLogsViewerConfigReader.ClientSecret ?? string.Empty;
                    var _ = this.lowLevelFFLogsClient.RefreshToken();
                }
            }

            if (ImGui.CollapsingHeader("How to get a Client ID and Secret:"))
            {
                ImGui.Bullet();
                ImGui.Text("Open https://www.fflogs.com/api/clients/ or");
                ImGui.SameLine();
                if (ImGui.Button("Click here##APIClientLink"))
                {
                    Util.OpenLink("https://www.fflogs.com/api/clients/");
                }

                ImGui.Bullet();
                ImGui.Text("Create a new client");
                ImGui.Bullet();
                ImGui.Text("Choose any name, for example: \"Plugin\"");
                ImGui.Bullet();
                ImGui.Text("Enter any URL, for example: \"https://www.example.com\"");
                ImGui.Bullet();
                ImGui.Text("Do NOT check the Public Client option");
                ImGui.Bullet();
                ImGui.Text("Paste both Client ID and Secret above");
            }
        }
    }
}
