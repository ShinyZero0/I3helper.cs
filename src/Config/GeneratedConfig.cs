namespace I3Helper;

public class GeneratedConfig
{
    public GeneratedConfig() { }

    public Dictionary<string, string> Variables { get; set; } = new();
    public Dictionary<string, string> KeyBindings { get; set; } = new();

    public GeneratedConfig GetCopy()
    {
        GeneratedConfig newConfig = new();
        newConfig.Variables = new(this.Variables);
        newConfig.KeyBindings = new(this.KeyBindings);
        return newConfig;
    }

    public GeneratedConfig GetMergedWith(GeneratedConfig anotherConfig)
    {
        // Logger.Log("Started merging");
        GeneratedConfig newConfig = this.GetCopy();
        foreach (KeyValuePair<string, string> kvpair in anotherConfig.Variables)
        {
            newConfig.Variables[kvpair.Key] = kvpair.Value;
            // Logger.Log($"Variable {kvpair.Key}: {kvpair.Value} ");
        }
        foreach (KeyValuePair<string, string> kvpair in anotherConfig.KeyBindings)
        {
            newConfig.KeyBindings[kvpair.Key] = kvpair.Value;
            // Logger.Log($"Binding {kvpair.Key}: {kvpair.Value} ");
        }
        return newConfig;
    }

    public void RewriteToFile(I3 i3)
    {
        List<string> content = new();
        foreach (KeyValuePair<string, string> entry in KeyBindings.Where(kv => kv.Value != null))
        {
            content.Add($"bindsym {entry.Key} {entry.Value}");
        }
        File.WriteAllLines(i3.GeneratedConfigPath, content);
        _ = i3.SendMessage("reload");
    }
}
