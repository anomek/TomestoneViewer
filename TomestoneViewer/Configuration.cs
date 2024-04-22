using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using TomestoneViewer.Model;
using Newtonsoft.Json;

namespace TomestoneViewer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 1;

    public int Version { get; set; } = CurrentConfigVersion;

    public string? DefaultEncounterDisplayName { get; set; }

    public void Save()
    {
        Service.Interface.SavePluginConfig(this);
    }
}
