using System.Text.Json.Serialization;
namespace I3IPC;
public class KeyBinding
{
    [JsonPropertyName("command")]
	[JsonInclude]
    public string Command;
}
