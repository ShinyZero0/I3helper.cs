using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

namespace I3Helper;

public partial class I3
{
    public Task NewBorderColorTask()
    {
        Task theTask =
            new(() =>
            {
                var generatedConfigPath = Path.Combine(this.ConfigDir, "generated");
                try
                {
                    generatedConfigPath = this.MainConfig.IncludedConfigs
                        .First(c => c.Path.Contains("generated"))
                        .Path;
                }
                catch (Exception) { }
                List<string> generatedConfigContent = new();
                this.Variables.TryAdd("active", "#aaffe4");
                this.Variables.TryAdd("indicator", "#63f2f1");
                this.Variables.TryAdd("border", "#aaffe4");
                this.Variables.TryAdd("alert", "#ff99e3");
                foreach (KeyValuePair<string, string> pair in this.Variables)
                {
                    var variable = new Variable(pair.Key, pair.Value);
                    generatedConfigContent.Add(variable.ToString());
                }
                // generatedConfigContent.Add("smart_borders off\n");
                generatedConfigContent.Add(
                    "client.focused $active $active $active $indicator $border"
                );

                var newGeneratedConfigContent = generatedConfigContent.ToList();
                for (int i = 0; i < newGeneratedConfigContent.Count(); i++)
                {
                    string current = newGeneratedConfigContent[i];
                    if (current.Contains("client.focused"))
                    {
                        string[] colorsSplitted = Regex.Split(current, @"\s+");
                        colorsSplitted[4] = "$alert";
                        current = String.Join(' ', colorsSplitted);
                    }
                    newGeneratedConfigContent[i] = current;
                }
                File.WriteAllLines(generatedConfigPath, newGeneratedConfigContent);
                this.SendMessage("reload");
                Thread.Sleep(2000);
                File.WriteAllLines(generatedConfigPath, generatedConfigContent);
                this.SendMessage("reload");
            });
        return theTask;
    }
}
