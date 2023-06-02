using System.Text.Json.Serialization;

namespace I3Helper;

public class Container
{
    [JsonPropertyName("id")]
    [JsonInclude]
    public long Id;

    [JsonPropertyName("window_properties")]
    [JsonInclude]
    public WindowProperties Properties;
}
