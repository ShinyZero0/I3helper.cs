namespace I3IPC;

public class GeneratedConfig : Config
{
    public GeneratedConfig(string path)
        : base(path) { }

    public Dictionary<string, string> Variables = new();
    public Dictionary<string, string> KeyBindings = new();

    public void Refresh(I3 i3)
    {
        List<string> content = new();
        foreach (KeyValuePair<string, string> entry in KeyBindings)
        {
            content.Add($"bindsym {entry.Key} {entry.Value}");
        }
        File.WriteAllLines(this.Path, content);
        i3.SendMessage("reload");
    }
}
