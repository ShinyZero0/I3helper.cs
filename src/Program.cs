using System.Text.Json;
using System.Reactive.Linq;

namespace I3Helper;

internal static class Program
{
    static I3 I3Instance;

    static async Task<int> Main(string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out _))
            throw new ArgumentException(
                "Need the port argument, got either non-number, not enough or too many arguments"
            );
        I3Instance = new();
        I3Instance.MainConfig = JsonSerializer.Deserialize<I3Config>(
            I3Instance.SendMessage("-t get_config --raw")
        )!;

        Task wschanges = I3Instance.MonitorWorkspaceChangesAsync();
        // await I3Instance.CheckBindingsAsync();

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapPost("/lastworkspace", () => I3Instance.GoToLastWorkspace());
        app.Run($"http://localhost:{args[0]}");

        return 0;
    }
}
