using System.Text.Json.Serialization;

namespace I3Helper;

public class Config
{
    [JsonPropertyName("variable_replaced_contents")]
    [JsonInclude]
    public string Content;

    [JsonPropertyName("path")]
    [JsonInclude]
    public string Path;

    public Config(string path)
    {
        this.Path = path;
    }
}
