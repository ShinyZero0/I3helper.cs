using Newtonsoft.Json;
using System.IO;
using System;

namespace I3IPC;

public class I3Config : Config
{
    [JsonProperty("included_configs")]
    public Config[] IncludedConfigs;
	public I3Config(string path) : base(path) {  }
}
