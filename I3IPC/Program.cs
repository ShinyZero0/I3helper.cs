using System.Diagnostics;
using System.Net.Sockets;
using System;
using System.IO;
using System.IO.Pipes;
using System.Reactive;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using MessagePipe;
using MessagePack.Internal;
using MessagePipe.Interprocess;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using H.Pipes;

namespace I3IPC;

internal class Program
{
    private static async Task Main(string[] args)
    {
        _messages["LastWorkspace"] = GoToLastWorkspace;
        var wsChecking = Task.Run(() => CheckWorkspaceChanges());
        await using var server = new PipeServer<string>("I3Server.cs");
        server.ClientConnected += async (o, args) =>
        {
            Console.WriteLine($"Client {args.Connection.PipeName} is now connected!");

            await args.Connection.WriteAsync("darova");
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

    public static List<Workspace> GetWorkspaces()
    {
        string json = SendMessage("-t get_workspaces");
        return JsonConvert.DeserializeObject<List<Workspace>>(json)!;
    }

    private static Dictionary<string, Action> _messages = new Dictionary<string, Action>();
    private static Action GoToLastWorkspace = () =>
    {
        SendMessage("workspace " + OldWorkspace.Name);
    };

    public static async void CheckWorkspaceChanges()
    {
        var changes = NewMessage(String.Format("-t subscribe [\"\"\"workspace\"\"\"]"));
        while (true)
        {
            changes.Start();
            Console.WriteLine("it started");
            changes.WaitForExit();
            var lastchange = JsonConvert.DeserializeObject<WorkspaceChanged>(
                changes.StandardOutput.ReadToEnd()
            );
            if (lastchange.Change == "focus")
            {
                OldWorkspace = lastchange.OldWorkspace;
                Console.WriteLine(lastchange.Change);
            }
        }
    }

    public static Workspace? OldWorkspace;

    // public Workspace NewWorkspace;
    // public static WorkspaceChanged SubscribeToWokrspaceChanges
    public static string SendMessage(string message)
    {
        Process i3message = NewMessage(message);
        i3message.Start();
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

public class WorkspaceChanged
{
    [JsonProperty("change")]
    public string Change;

    [JsonProperty("old")]
    public Workspace? OldWorkspace;

    [JsonProperty("current")]
    public Workspace? NewWorkspace;
}
