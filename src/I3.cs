using System.Diagnostics;
using System.Reactive.Linq;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace I3Helper;

public partial class I3
{
    Workspace? OldWorkspace;
    public I3Config MainConfig;
    GeneratedConfig GeneratedConfig;
    Dictionary<string, string> Variables = new();
    string ConfigDir;

    public I3()
    {
        ConfigDir = Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".config", "i3");
        GeneratedConfig = new(Path.Combine(ConfigDir, "generated"));
        // Wait in case i3 don't accepts IPC commands the moment it's started
        _ = SendMessage("-t get_version");
        this.MainConfig = JsonSerializer.Deserialize<I3Config>(
            _ = this.SendMessage("-t get_config --raw")
        )!;
    }

    public string SendMessage(string message)
    {
        using (Process i3message = NewMessage(message))
        {
            i3message.Start();
            i3message.WaitForExit();
            string result = i3message.StandardOutput.ReadToEnd();
            i3message.StandardError.ReadToEnd();
            i3message.Dispose();
            return result;
        }
    }

    public Process NewMessage(string args)
    {
        Process i3message = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "i3-msg",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        return i3message;
    }

    // public void CheckWindowChanges()
    // {
    //     var subscription = NewMessage(Commands.Subscribe("window"));
    //     while (true)
    //     {
    //         subscription.Start();
    //         subscription.WaitForExit();
    //         var change = JsonConvert.DeserializeObject<WindowChanged>(
    //             subscription.StandardOutput.ReadToEnd()
    //         );
    //         if (change.Change == WindowChanged.ChangeTypes.New) { }
    //     }
    // }

    public async Task MonitorWorkspaceChangesAsync()
    {
        using (Process sub = NewMessage(Commands.Subscribe("workspace")))
        {
            sub.Start();
            var reactiveSub = Observable.FromEventPattern<
                DataReceivedEventHandler,
                DataReceivedEventArgs
            >((hand) => sub.OutputDataReceived += hand, (hand) => sub.OutputDataReceived -= hand);
            var disposableSub = reactiveSub.Subscribe(info =>
            {
                WorkspaceChanged change = JsonSerializer.Deserialize<WorkspaceChanged>(
                    info.EventArgs.Data!
                )!;
                if (change.Change == "focus")
                {
                    OldWorkspace = change.OldWorkspace;
                    if (change.NewWorkspace!.Name == "3")
                    {
                        this.GeneratedConfig.KeyBindings.Remove("Next");
                        this.GeneratedConfig.KeyBindings.Remove("Prior");
                        GeneratedConfig.Refresh(this);
                    }
                    else
                    {
                        this.GeneratedConfig.KeyBindings["Prior"] =
                            "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu up 5\"";
                        this.GeneratedConfig.KeyBindings["Next"] =
                            "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu down 5\"";
                        GeneratedConfig.Refresh(this);
                    }
                }
            });
            sub.BeginOutputReadLine();
            Task wait = sub.WaitForExitAsync();
            await wait;
            wait.Dispose();
            disposableSub.Dispose();
        }
    }

    public async Task CheckBindingsAsync()
    {
        using (Process sub = this.NewMessage(Commands.Subscribe("binding")))
        {
            sub.Start();
            var reactiveSub = Observable.FromEventPattern<
                DataReceivedEventHandler,
                DataReceivedEventArgs
            >((hand) => sub.OutputDataReceived += hand, (hand) => sub.OutputDataReceived -= hand);
            var disposableSub = reactiveSub.Subscribe(async info =>
            {
                var change = JsonSerializer.Deserialize<BindingChanged>(info.EventArgs.Data);
                // if (change.Binding.Command.Contains("split"))
                // {
                //     var task = NewBorderColorTask();
                //     task.Start();
                //     await task;
                //     task.Dispose();
                // }
            });
            sub.BeginOutputReadLine();
            await sub.WaitForExitAsync().ConfigureAwait(false);
            sub.CancelOutputRead();
            await sub.StandardOutput.ReadToEndAsync();
            await sub.StandardError.ReadToEndAsync();
            disposableSub.Dispose();
        }
    }

    public List<Workspace> GetWorkspaces()
    {
        string json = SendMessage("-t get_workspaces");
        return JsonSerializer.Deserialize<List<Workspace>>(json)!;
    }

    public void GoToLastUsedWorkspace()
    {
        if (OldWorkspace != null)
            _ = SendMessage("workspace " + OldWorkspace.Name);
    }

    public void GoToLastWorkspace()
    {
        List<Workspace> workspaces = GetWorkspaces();
        _ = SendMessage(
            "workspace " + workspaces.First(ws => ws.Num == workspaces.Max(ws => ws.Num)).Name
        );
    }

    void DetectByteOrder()
    {
        string byteorder;
        string unixSocket = Environment.GetEnvironmentVariable("I3SOCK");
        Socket sock = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
        UnixDomainSocketEndPoint ep = new(unixSocket);
        sock.Connect(ep);
        sock.Send(Encoding.ASCII.GetBytes(""));
    }
}
