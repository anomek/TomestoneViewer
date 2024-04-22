using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomestoneViewer.Model.GameData;

public class Expansion
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("id")]
    public int? Id { get; set; }

    [JsonProperty("zones")]
    public List<Zone>? Zones { get; set; }
}
