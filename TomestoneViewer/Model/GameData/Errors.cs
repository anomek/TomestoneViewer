using Newtonsoft.Json;

namespace TomestoneViewer.Model.GameData;

public class Errors
{
    [JsonProperty("message")]
    public string? Message { get; set; }
}
