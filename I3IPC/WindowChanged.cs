using Newtonsoft.Json;

namespace I3IPC;

public class WindowChanged
{
    [JsonProperty("change")]
    public ChangeTypes Change;

    [JsonProperty("container")]
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
