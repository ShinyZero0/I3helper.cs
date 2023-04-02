using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Linq;

namespace I3IPC;

public partial class I3
{
    public Workspace? OldWorkspace;
    public I3Config MainConfig;
    public Dictionary<string, string> Variables = new();
    public string ConfigDir;
    public string GeneratedConfigDir => Path.Combine(ConfigDir, "generated") ;

    // public I3()
    // {
    // }
    public string SendMessage(string message)
    {
        Process i3message = NewMessage(message);
        i3message.Start();
        i3message.WaitForExit();
        string result = i3message.StandardOutput.ReadToEnd();
        i3message.Dispose();
        return result;
    }

    public Process NewMessage(string args)
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

    public async void CheckWindowChanges()
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

    public async void CheckWorkspaceChanges()
    {
        var sub = NewMessage(Commands.Subscribe("workspace"));
        sub.Start();
        var reactiveSub = Observable.FromEventPattern<
            DataReceivedEventHandler,
            DataReceivedEventArgs
        >((hand) => sub.OutputDataReceived += hand, (hand) => sub.OutputDataReceived -= hand);
        var disposableSub = reactiveSub.Subscribe(info =>
        {
            var change = JsonConvert.DeserializeObject<WorkspaceChanged>(info.EventArgs.Data);
            if (change.Change == "focus")
            {
                OldWorkspace = change.OldWorkspace;
            }
        });
        sub.BeginOutputReadLine();
        await sub.WaitForExitAsync();
        sub.Dispose();
        disposableSub.Dispose();
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
                Console.WriteLine("Event received");
                var change = JsonConvert.DeserializeObject<BindingChanged>(info.EventArgs.Data);
                if (change.Binding.Command.Contains("split"))
                {
                    // BorderColorTaskFactory.DisposeTasks();
                    var task = NewBorderColorTask();
                    task.Start();
                    await task;
                    task.Dispose();
                }
            });
            sub.BeginOutputReadLine();
            await sub.WaitForExitAsync();
            disposableSub.Dispose();
        }
    }
    public void Swallow() { }


    public List<Workspace> GetWorkspaces()
    {
        string json = SendMessage("-t get_workspaces");
        return JsonConvert.DeserializeObject<List<Workspace>>(json)!;
    }

    public void GoToLastWorkspace()
    {
        if (OldWorkspace != null)
            SendMessage("workspace " + OldWorkspace.Name);
    }
}
