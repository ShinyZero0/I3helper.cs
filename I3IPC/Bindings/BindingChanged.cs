using Newtonsoft.Json;
namespace I3IPC;
public class BindingChanged
{
    [JsonProperty("binding")]
    public KeyBinding Binding;
}
