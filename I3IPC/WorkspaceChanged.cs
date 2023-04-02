using Newtonsoft.Json;
namespace I3IPC;
public class WorkspaceChanged
{
    [JsonProperty("change")]
    public string Change;

    [JsonProperty("old")]
    public Workspace? OldWorkspace;

    [JsonProperty("current")]
    public Workspace? NewWorkspace;
}
