using System.Text.Json.Serialization;

namespace I3Helper;

public class Config
{
    [JsonPropertyName("variable_replaced_contents")]
    public string Content { get; set; }

    [JsonPropertyName("path")]
    public string Path { init; get; }

    public Config(string path)
    {
        this.Path = path;
    }
}
