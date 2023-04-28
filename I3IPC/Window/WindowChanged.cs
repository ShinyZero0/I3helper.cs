using System.Text.Json.Serialization;

namespace I3IPC;

public class WindowChanged
{
    [JsonPropertyName("change")]
	[JsonInclude]
    public ChangeTypes Change;

    [JsonPropertyName("container")]
	[JsonInclude]
    public Container Container;

    public enum ChangeTypes
    {
        Close,
        New,
        Focus,
        Title,
        Move,
        Floating,
        FullscreenMode,
    }
}
