using System.Text.Json.Serialization;

public class WindowProperties
{
    [JsonPropertyName("class")]
    [JsonInclude]
    public string Class;
}
