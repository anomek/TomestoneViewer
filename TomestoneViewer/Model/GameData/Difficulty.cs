using Newtonsoft.Json;

namespace TomestoneViewer.Model.GameData;

public class Difficulty
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("id")]
    public int? Id { get; set; }
}
