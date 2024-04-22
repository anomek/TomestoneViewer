using Newtonsoft.Json;

namespace TomestoneViewer.Model.GameData;

public class Data
{
    [JsonProperty("worldData")]
    public WorldData? WorldData { get; set; }
}
