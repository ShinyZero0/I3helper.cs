using Newtonsoft.Json;

namespace I3IPC;
public class Config
{
    [JsonProperty("variable_replaced_contents")]
    public string Content;
    [JsonProperty("path")]
    public string Path;
	public Config(string path)
	{
		this.Path = path;
	}
}
