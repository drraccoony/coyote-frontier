using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared._NF.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._NF.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class ToggleBluespaceDockingCommand : IConsoleCommand
{
    public string Command => "togglebluespacedocking";
    public string Description => Loc.GetString("toggle-bluespace-docking-command-description");
    public string Help => Loc.GetString("toggle-bluespace-docking-command-help");
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var cfg = IoCManager.Resolve<IConfigurationManager>();

        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("toggle-bluespace-docking-command-too-many-arguments-error"));
            return;
        }

        var enabled = cfg.GetCVar(NFCCVars.BluespaceDockingEnabled);

        if (args.Length == 0)
        {
            enabled = !enabled;
        }

        if (args.Length == 1 && !bool.TryParse(args[0], out enabled))
        {
            shell.WriteError(Loc.GetString("toggle-bluespace-docking-command-invalid-argument-error"));
            return;
        }

        cfg.SetCVar(NFCCVars.BluespaceDockingEnabled, enabled);

        shell.WriteLine(Loc.GetString(enabled ? "toggle-bluespace-docking-command-enabled" : "toggle-bluespace-docking-command-disabled"));
    }
}
