using System.Text.Json.Serialization;

namespace I3IPC;

public class Workspace
{
    public void Display()
    {
        Console.WriteLine(this.Name);
    }

    [JsonPropertyName("id")]
	[JsonInclude]
    public long Id;

    [JsonPropertyName("num")]
	[JsonInclude]
    public int Num;

    [JsonPropertyName("name")]
	[JsonInclude]
    public string Name;

    [JsonPropertyName("visible")]
	[JsonInclude]
    public bool IsVisible;

    [JsonPropertyName("focused")]
	[JsonInclude]
    public bool IsFocused;

    [JsonPropertyName("urgent")]
	[JsonInclude]
    public bool IsUrgent;
}
