# Intro

It extends some i3wm functionality. Currently there are 3 functions:

* Temporarily highlight the indicator border when changing the split direction (not maintained and disabled because i'm too lazy to make it work together with `smart_borders`).
* Add command for switching to last used workspace.
* Add command for switching to last (just last) workspace.
* Change PgUp and PgDn bindings depending on workspace number to scroll half-page in all WS except 3rd where i play. Yes, it's hardcoded.

# Usage

Only one command is provided: `I3Helper`.

You launch it with i3wm and it sits and does its work in background.
You need to specify the port as the only argument.

```bash
I3Helper 13131
```

For last workspaces functions you need to send http POST to its localhost api endpoint like

```bash
curl -X POST http://localhost:13131/lastworkspace # for last in numerical way
curl -X POST http://localhost:13131/lastworkspace # for last used
```

Ofc it's expected you will bind these in i3 config. E.g. i have Super+E for End and Super+R for Return or Recent
# Btw 

It can use 100-200mb of RAM if you have much free memory.
When you have less than 10% free it shrinks to 15-20mb.
That's how .NET (and maybe not only its) GC works.
