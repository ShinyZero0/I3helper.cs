using Newtonsoft.Json;

namespace I3IPC;

public class Container
{
    [JsonProperty("id")]
    long Id;
	[JsonProperty("window_properties")]
	WindowProperties Properties;
}
