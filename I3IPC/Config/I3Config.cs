using System.Text.Json.Serialization;
using System.IO;
using System;

namespace I3IPC;

public class I3Config : Config
{
    [JsonPropertyName("included_configs")]
    [JsonInclude]
    public Config[] IncludedConfigs;

    public I3Config(string path)
        : base(path) { }
}
