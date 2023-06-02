using System.Diagnostics;
using System.Reactive.Linq;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace I3Helper;

public partial class I3
{
    I3Config _mainConfig;
    private GeneratedConfig __generatedConfig;
    private GeneratedConfig _generatedConfig
    {
        get => this.__generatedConfig;
        set
        {
            this.__generatedConfig = value;
            this.__generatedConfig.RewriteToFile(this);
            // _log("GeneratedConfig value updated");
        }
    }
    GeneratedConfig _defaultConfig;
    public string GeneratedConfigPath;

    private ReadOnlyDictionary<string, GeneratedConfig> _windowSpecificConfigs { get; init; }

    private async Task _log(string msg)
    {
        await Logger.Log(msg);
    }

    public I3()
    {
        string configHome = Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".config");
        string i3configDir = Path.Combine(configHome, "i3");
        GeneratedConfigPath = Path.Combine(i3configDir, "generated");
        _generatedConfig = new();
        // Wait in case i3 don't accepts IPC commands the moment it's started
        _ = SendMessage("-t get_version");
        this._mainConfig = JsonSerializer.Deserialize<I3Config>(
            _ = this.SendMessage("-t get_config --raw")
        )!;
        this._defaultConfig = new GeneratedConfig();
        this._defaultConfig.KeyBindings["Prior"] =
            "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu up 5\"";
        this._defaultConfig.KeyBindings["Next"] =
            "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu down 5\"";
        this._windowSpecificConfigs = new(
            JsonSerializer.Deserialize<Dictionary<string, GeneratedConfig>>(
                File.ReadAllText(Path.Combine(configHome, "i3helperwinspecs.json"))
            )
        );
    }

    public async Task MonitorWindowChangesAsync()
    {
        I3Socket socket = new();
        await socket.SubscribeAsync("window");
        // _log(socket.IsReadable.ToString());
        while (socket.IsReadable)
        {
            Task<string> outputTask = socket.ReadOutputAsync();
            WindowChanged change = JsonSerializer.Deserialize<WindowChanged>(await outputTask);
            // await _log("event handled");
            if (
                change.Change == WindowChanged.ChangeTypes.New
                || change.Change == WindowChanged.ChangeTypes.Focus
            )
            {
                string winClass = change.Container.Properties.Class;
                bool contains = false;
                foreach (string key in _windowSpecificConfigs.Keys)
                {
                    if (Regex.IsMatch(winClass, key))
                    {
                        contains = true;
                        this._generatedConfig = this._defaultConfig
                            .GetCopy()
                            .GetMergedWith(_windowSpecificConfigs[key]);
                        // Logger.Log("matched the window class");
                        break;
                    }
                }
                if (!contains)
                {
                    this._generatedConfig = this._defaultConfig.GetCopy();
                    // _log("set to default");
                }
            }
        }
    }

    // public async Task MonitorWorkspaceChangesAsync()
    // {
    //     using (Process sub = NewMessage(Commands.Subscribe("workspace")))
    //     {
    //         sub.Start();
    //         var reactiveSub = Observable.FromEventPattern<
    //             DataReceivedEventHandler,
    //             DataReceivedEventArgs
    //         >((hand) => sub.OutputDataReceived += hand, (hand) => sub.OutputDataReceived -= hand);
    //         var disposableSub = reactiveSub.Subscribe(info =>
    //         {
    //             WorkspaceChanged change = JsonSerializer.Deserialize<WorkspaceChanged>(
    //                 info.EventArgs.Data!
    //             )!;
    //             if (change.Change == "focus")
    //             {
    //                 if (change.NewWorkspace!.Name == "3")
    //                 {
    //                     this._generatedConfig.KeyBindings.Remove("Next");
    //                     this._generatedConfig.KeyBindings.Remove("Prior");
    //                     // _generatedConfig.Refresh(this);
    //                 }
    //                 else
    //                 {
    //                     this._generatedConfig.KeyBindings["Prior"] =
    //                         "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu up 5\"";
    //                     this._generatedConfig.KeyBindings["Next"] =
    //                         "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu down 5\"";
    //                     // _generatedConfig.Refresh(this);
    //                 }
    //             }
    //         });
    //         sub.BeginOutputReadLine();
    //         Task wait = sub.WaitForExitAsync();
    //         await wait;
    //         wait.Dispose();
    //         disposableSub.Dispose();
    //     }
    // }

    // public async Task CheckBindingsAsync()
    // {
    //     using (Process sub = this.NewMessage(Commands.Subscribe("binding")))
    //     {
    //         sub.Start();
    //         var reactiveSub = Observable.FromEventPattern<
    //             DataReceivedEventHandler,
    //             DataReceivedEventArgs
    //         >((hand) => sub.OutputDataReceived += hand, (hand) => sub.OutputDataReceived -= hand);
    //         var disposableSub = reactiveSub.Subscribe(async info =>
    //         {
    //             var change = JsonSerializer.Deserialize<BindingChanged>(info.EventArgs.Data);
    //             // if (change.Binding.Command.Contains("split"))
    //             // {
    //             //     var task = NewBorderColorTask();
    //             //     task.Start();
    //             //     await task;
    //             //     task.Dispose();
    //             // }
    //         });
    //         sub.BeginOutputReadLine();
    //         await sub.WaitForExitAsync().ConfigureAwait(false);
    //         sub.CancelOutputRead();
    //         await sub.StandardOutput.ReadToEndAsync();
    //         await sub.StandardError.ReadToEndAsync();
    //         disposableSub.Dispose();
    //     }
    // }

    public List<Workspace> GetWorkspaces()
    {
        string json = SendMessage("-t get_workspaces");
        return JsonSerializer.Deserialize<List<Workspace>>(json)!;
    }

    public void GoToLastWorkspace()
    {
        List<Workspace> workspaces = GetWorkspaces();
        _ = SendMessage(
            "workspace " + workspaces.First(ws => ws.Num == workspaces.Max(ws => ws.Num)).Name
        );
    }

    public Process NewMessage(string args)
    {
        Process i3message = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/i3-msg",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        return i3message;
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
}
