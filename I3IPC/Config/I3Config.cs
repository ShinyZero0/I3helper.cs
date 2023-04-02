using Newtonsoft.Json;
using System.IO;
using System;

namespace I3IPC;

public class I3Config
{
    [JsonProperty("included_configs")]
    public Config[] IncludedConfigs;
}
