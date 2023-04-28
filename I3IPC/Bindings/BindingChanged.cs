using System.Text.Json.Serialization;
namespace I3IPC;
public class BindingChanged
{
    [JsonPropertyName("binding")]
	[JsonInclude]
    public KeyBinding Binding;
}
