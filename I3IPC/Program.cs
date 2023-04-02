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
        I3Instance = new();
        I3Instance.ConfigDir = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config", "i3");
        I3Instance.SendMessage("-t get_version");

        I3Instance.MainConfig = JsonConvert.DeserializeObject<I3Config>(
            I3Instance.SendMessage("-t get_config --raw")
        );
        foreach (var entry in I3Instance.MainConfig.IncludedConfigs)
        {
            Console.WriteLine(entry.Path);
        }

        var wsChecking = Task.Run(() => I3Instance.CheckWorkspaceChanges());
        var bindChecking = Task.Run(() => I3Instance.CheckBindings());
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

        await Task.Run(() =>
        {
            while (true)
                Thread.Sleep(TimeSpan.FromDays(10));
        });
        await wsChecking;
        await bindChecking;
        await server.StopAsync();
        await server.DisposeAsync();
    }
private static Dictionary<string, Action> _messages = new Dictionary<string, Action>()
{
    { "LastWorkspace", () => I3Instance.GoToLastWorkspace() }
};
private static I3 I3Instance;

}
