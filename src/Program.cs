﻿using System.Text.Json;
using System.Reactive.Linq;

namespace I3Helper;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        I3Instance = new();
        I3Instance.MainConfig = JsonSerializer.Deserialize<I3Config>(
            I3Instance.SendMessage("-t get_config --raw")
        )!;

        Console.WriteLine("starting CheckWorkspaceChangesAsync");
        Task wschanges = I3Instance.MonitorWorkspaceChangesAsync();
        // await I3Instance.CheckBindingsAsync();
        Console.WriteLine("starting webapp");

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapPost("/lastworkspace", () => I3Instance.GoToLastWorkspace());
        app.Run();

        // await using var server = new PipeServer<string>(
        //     "I3Server.cs",
        //     formatter: new SystemTextJsonFormatter()
        // );
        // server.ClientConnected += async (o, args) =>
        // {
        //     Console.WriteLine($"Client {args.Connection.PipeName} is now connected!");
        // };
        //
        // server.ClientDisconnected += (o, args) =>
        // {
        //     Console.WriteLine($"Client {args.Connection.PipeName} disconnected");
        // };
        // server.MessageReceived += (sender, args) =>
        // {
        //     Console.WriteLine($"Client {args.Connection.PipeName} says: {args.Message}");
        //     if (_messages.ContainsKey(args.Message))
        //         _messages[args.Message]();
        // };
        //
        // await server.StartAsync();

        // await Task.Run(() =>
        // {
        //     while (true)
        //         Thread.Sleep(TimeSpan.FromDays(10));
        // });
        // await server.StopAsync();
        // await server.DisposeAsync();
    }

    static I3 I3Instance;
    // static Dictionary<string, Action> _messages = new Dictionary<string, Action>()
    // {
    //     { "LastWorkspace", () => I3Instance.GoToLastWorkspace() }
    // };
}
