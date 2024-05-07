using System;

using Dalamud.Configuration;
using Newtonsoft.Json;

namespace TomestoneViewer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 1;

    public int Version { get; set; } = CurrentConfigVersion;

    public void Save()
    {
        Service.Interface.SavePluginConfig(this);
    }
}
