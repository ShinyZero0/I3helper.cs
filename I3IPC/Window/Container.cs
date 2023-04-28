using System.Text.Json.Serialization;

namespace I3IPC;

public class Container
{
    [JsonPropertyName("id")]
    [JsonInclude]
    long Id;

    [JsonPropertyName("window_properties")]
    [JsonInclude]
    WindowProperties Properties;
}
