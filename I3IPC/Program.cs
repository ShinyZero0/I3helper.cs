using System.Diagnostics;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using System.Reactive.Linq;
using H.Pipes;
using H.Formatters;

namespace I3IPC;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        SendMessage("-t get_version");

        MainConfig = JsonConvert.DeserializeObject<I3Config>(SendMessage("-t get_config --raw"));
        foreach (var entry in MainConfig.IncludedConfigs)
        {
            Console.WriteLine(entry.Path);
        }

        var wsChecking = Task.Run(() => CheckWorkspaceChanges());
        var bindChecking = Task.Run(() => CheckBindings());
        await using var server = new PipeServer<string>(
            "I3Server.cs",
            formatter: new SystemTextJsonFormatter()
        );
        server.ClientConnected += async (o, args) =>
        {
            Console.WriteLine($"Client {args.Connection.PipeName} is now connected!");
        };

        server.ClientDisconnected += (o, args) =>
        {
            Console.WriteLine($"Client {args.Connection.PipeName} disconnected");
        };
        server.MessageReceived += (sender, args) =>
        {
            Console.WriteLine($"Client {args.Connection.PipeName} says: {args.Message}");
            if (_messages.ContainsKey(args.Message))
                _messages[args.Message]();
        };
        // server.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

        await server.StartAsync();

        await wsChecking;
    }

    public static void Swallow() { }

    public static List<Workspace> GetWorkspaces()
    {
        string json = SendMessage("-t get_workspaces");
        return JsonConvert.DeserializeObject<List<Workspace>>(json)!;
    }

    private static Dictionary<string, Action> _messages = new Dictionary<string, Action>()
    {
        { "LastWorkspace", GoToLastWorkspace }
    };

    private static void GoToLastWorkspace()
    {
        if (OldWorkspace != null)
            SendMessage("workspace " + OldWorkspace.Name);
    }

    public static async void CheckWindowChanges()
    {
        var subscription = NewMessage(Commands.Subscribe("window"));
        while (true)
        {
            subscription.Start();
            subscription.WaitForExit();
            var change = JsonConvert.DeserializeObject<WindowChanged>(
                subscription.StandardOutput.ReadToEnd()
            );
            if (change.Change == WindowChanged.ChangeTypes.New) { }
        }
    }

    public static async void CheckWorkspaceChanges()
    {
        var subscription = NewMessage(Commands.Subscribe("workspace"));
        while (true)
        {
            subscription.Start();
            subscription.WaitForExit();
            var change = JsonConvert.DeserializeObject<WorkspaceChanged>(
                subscription.StandardOutput.ReadToEnd()
            );
            if (change.Change == "focus")
            {
                OldWorkspace = change.OldWorkspace;
            }
        }
    }

    public static async void CheckBindings()
    {
        var subscription = NewMessage(Commands.Subscribe("binding"));
        while (true)
        {
            subscription.Start();
            subscription.WaitForExit();
            var change = JsonConvert.DeserializeObject<BindingChanged>(
                subscription.StandardOutput.ReadToEnd()
            );
            if (change.Binding.Command.Contains("split"))
            {
                Task changeBorderColorTask = ChangeBorderColor();
                changeBorderColorTask.Start();
                Console.WriteLine("task started");
            }
        }
    }

    private static Task ChangeBorderColor() =>
        new(() =>
        {
            var generatedConfigPath = MainConfig.IncludedConfigs.First(
                c => c.Path.Contains("generated")
            ).Path;
            List<string> generatedConfigContent = new();
            generatedConfigContent.Add("set $active #aaffe4");
            generatedConfigContent.Add("set $indicator #63f2f1");
            generatedConfigContent.Add("set $border #aaffe4");
            generatedConfigContent.Add("set $alert #ff99e3");
            generatedConfigContent.Add("client.focused $active $active $active $indicator $border");
            
            var newGeneratedConfigContent = generatedConfigContent.ToList();
            for (int i = 0; i < newGeneratedConfigContent.Count(); i++)
            {
                string current = newGeneratedConfigContent[i];
                if (current.Contains("client.focused"))
                {
                    Console.WriteLine("client.focused found");
                    string[] colorsSplitted = Regex.Split(current, @"\s+");
                    colorsSplitted[4] = "$alert";
                    foreach (var entry in colorsSplitted)
                    {
                        Console.WriteLine(entry);
                    }
                    current = String.Join(' ', colorsSplitted);
                    Console.WriteLine("Current string is" + current);
                }
                newGeneratedConfigContent[i] = current;
            }
            File.WriteAllLines(generatedConfigPath, newGeneratedConfigContent);
            SendMessage("-t command reload");
            Thread.Sleep(2000);
            Console.WriteLine("sleeped");
            File.WriteAllLines(generatedConfigPath, generatedConfigContent);
            generatedConfigContent.ForEach(Console.WriteLine);
            SendMessage("-t command reload");
            return;
        });

    public static Workspace? OldWorkspace;
    private static I3Config MainConfig;

    public static string SendMessage(string message)
    {
        Process i3message = NewMessage(message);
        i3message.Start();
        i3message.WaitForExit();
        return i3message.StandardOutput.ReadToEnd();
    }

    private static Process NewMessage(string args)
    {
        Process i3message = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "i3-msg",
                Arguments = args,
                // UseShellExecute = true
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        return i3message;
    }
}
