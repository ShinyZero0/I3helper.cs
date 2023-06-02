using System.Text.Json;
using System.Reactive.Linq;

namespace I3Helper;

internal static class Program
{
    static I3 I3Instance;

    static async Task Main(string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out _))
            throw new ArgumentException(
                "Need the port argument, got either non-number, not enough or too many arguments"
            );
        I3Instance = new();

        List<Task> tasks = new();
        // Task wschanges = I3Instance.MonitorWorkspaceChangesAsync();
        tasks.Add(I3Instance.MonitorWindowChangesAsync());
        // await I3Instance.CheckBindingsAsync();

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapPost("/lastworkspace", () => I3Instance.GoToLastWorkspace());
        tasks.Add(app.RunAsync($"http://localhost:{args[0]}"));
        await Task.WhenAll(tasks);
    }
}
