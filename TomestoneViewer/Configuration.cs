using System;

using Dalamud.Configuration;
using Newtonsoft.Json;

namespace TomestoneViewer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 2;

    public int Version { get; set; } = CurrentConfigVersion;

    public bool FFLogsEnabled { get; set; } = true;

    public bool UseDefaultFont { get; set; } = false;

    public bool StreamerMode { get; set; } = false;

    public bool PlayerTrackEnabled { get; set; } = true;

    public string FFLogsClientId { get; set; } = string.Empty;

    public string FFLogsClientSecret { get; set; } = string.Empty;

    public void Save()
    {
        Service.Interface.SavePluginConfig(this);
    }
}
