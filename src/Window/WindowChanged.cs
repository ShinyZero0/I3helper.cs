using System.Text.Json.Serialization;

namespace I3Helper;

public class WindowChanged
{
    [JsonPropertyName("change")]
    [JsonInclude]
    public ChangeTypes Change;

    [JsonPropertyName("container")]
    [JsonInclude]
    public Container Container;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChangeTypes
    {
        [JsonPropertyName("new")]
        New,

        [JsonPropertyName("close")]
        Close,

        [JsonPropertyName("focus")]
        Focus,

        [JsonPropertyName("title")]
        Title,

        [JsonPropertyName("move")]
        Move,

        [JsonPropertyName("floating")]
        Floating,

        [JsonPropertyName("fullscreenmode")]
        FullscreenMode,

        [JsonPropertyName("urgent")]
        Urgent,

        [JsonPropertyName("mark")]
        Mark
    }
}
