using System.Text.Json.Serialization;
namespace I3IPC;
public class WorkspaceChanged
{
    [JsonPropertyName("change")]
	[JsonInclude]
    public string Change;

    [JsonPropertyName("old")]
	[JsonInclude]
    public Workspace? OldWorkspace;

    [JsonPropertyName("current")]
	[JsonInclude]
    public Workspace? NewWorkspace;
}
