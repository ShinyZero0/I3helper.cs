using System.Text.Json.Serialization;

namespace I3Helper;

public class Container
{
    [JsonPropertyName("id")]
    [JsonInclude]
    long Id;

    [JsonPropertyName("window_properties")]
    [JsonInclude]
    WindowProperties Properties;
}
