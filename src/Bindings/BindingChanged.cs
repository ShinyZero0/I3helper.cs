using System.Text.Json.Serialization;

namespace I3Helper;

public class BindingChanged
{
    [JsonPropertyName("binding")]
    [JsonInclude]
    public KeyBinding Binding;
}
