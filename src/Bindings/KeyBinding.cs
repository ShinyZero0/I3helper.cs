using System.Text.Json.Serialization;

namespace I3Helper;

public class KeyBinding
{
    [JsonPropertyName("command")]
    [JsonInclude]
    public string Command;
}
