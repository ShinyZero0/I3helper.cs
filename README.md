# Intro

It extends some i3wm functionality. Currently there are 3 functions:

* Temporarily highlight the indicator border when changing the split direction (not maintained and disabled because i'm too lazy to make it work together with `smart_borders`).
* Add command for switching to last used workspace.
* Add command for switching to last (just last) workspace.
* Change PgUp and PgDn bindings depending on workspace number to scroll half-page in all WS except 3rd where i play. Yes, it's hardcoded.

# Usage

Only one command is provided: `I3Helper`.

Launch it with i3wm and it will do its work in background.
You need to specify the port as the only argument:

```bash
I3Helper 13131
```

For last workspaces function you need to send http POST to its localhost api endpoints like

```bash
curl -X POST http://localhost:13131/lastworkspace # last in numerical way
```

Ofc it's expected you will bind these in i3 config. E.g. i have Super+E for lastworkspace

## Remaps based on window class

it reads the config file in ~/.config/i3helperwinspecs.json which have the following structure:

```json
{
    "TelegramDesktop": {
        "KeyBindings": {
            "Control+D": "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu down 5\"",
            "Control+U": "exec --no-startup-id \"nu --no-std-lib ~/.scripts/HalfPage.nu up 5\""
        }
    }
}
```

Key is a window class, value is a object with dictionary "KeyBindings", where keys are keys and values are executed commands.
These are inserted into `~/.config/i3/generated` file which you should `include`, bindings are inserted as is.

# Btw

It can use up to ∞ mb of RAM if you have much free memory.
When you have less than 10% free it shrinks to 15-20mb.
That's how .NET (maybe not only its) GC works.

# TODO:

- [ ] Check if the config is already default and not try to overwrite it
- [ ] Add endpoint to reload its config.
- [ ] Unhardcode my personal config from defaults.
