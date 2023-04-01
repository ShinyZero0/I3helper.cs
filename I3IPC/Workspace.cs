using Newtonsoft.Json;

namespace I3IPC;

public class Workspace
{
    public void Display()
    {
        Console.WriteLine(this.Name);
    }

    [JsonProperty("id")]
    public long Id;

    [JsonProperty("num")]
    public int Num;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("visible")]
    public bool IsVisible;

    [JsonProperty("focused")]
    public bool IsFocused;

    [JsonProperty("urgent")]
    public bool IsUrgent;
}
