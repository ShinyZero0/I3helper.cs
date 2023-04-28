using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace I3IPC;

public partial class I3
{
    Workspace? OldWorkspace;
    public I3Config MainConfig;
    GeneratedConfig GeneratedConfig;
    Dictionary<string, string> Variables = new();
    string ConfigDir;

    public I3()
    {
        ConfigDir = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config", "i3");
        GeneratedConfig = new(Path.Combine(ConfigDir, "generated"));
        SendMessage("-t get_version");
    }

    public string SendMessage(string message)
    {
        using (Process i3message = NewMessage(message))
        {
            i3message.Start();
            i3message.WaitForExit();
            string result = i3message.StandardOutput.ReadToEnd();
			GC.Collect();
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

    public async void CheckWorkspaceChanges()
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
                            "exec --no-startup-id \"~/.scripts/HalfPage.nu up\"";
                        this.GeneratedConfig.KeyBindings["Next"] =
                            "exec --no-startup-id \"~/.scripts/HalfPage.nu down\"";
                        GeneratedConfig.Refresh(this);
                    }
                }
				GC.Collect();
            });
            sub.BeginOutputReadLine();
            Task wait = sub.WaitForExitAsync();
            await wait;
            wait.Dispose();
            disposableSub.Dispose();
        }
    }

    public async void CheckBindings()
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
            disposableSub.Dispose();
        }
    }

    public List<Workspace> GetWorkspaces()
    {
        string json = SendMessage("-t get_workspaces");
        return JsonSerializer.Deserialize<List<Workspace>>(json)!;
    }

    public void GoToLastWorkspace()
    {
        if (OldWorkspace != null)
            SendMessage("workspace " + OldWorkspace.Name);
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
