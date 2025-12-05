using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    sealed class SetShiftEndTimeCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _e = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        public string Command => "setshiftendtime";
        public string Description => "Sets the shift end time in hours from now (uses server real time to avoid drift).";
        public string Help => "setshiftendtime <hours> - Sets when the shift should end. Use 0 to clear.";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var ticker = _e.System<GameTicker>();

            if (ticker.RunLevel != GameRunLevel.InRound)
            {
                shell.WriteLine("This can only be executed while the game is in a round.");
                return;
            }

            if (args.Length < 1)
            {
                shell.WriteError("Expected at least 1 argument.");
                shell.WriteLine(Help);
                return;
            }

            if (!double.TryParse(args[0], out var hours))
            {
                shell.WriteError("Invalid number format for hours.");
                return;
            }

            if (hours <= 0)
            {
                ticker.ShiftEndTime = null;
                shell.WriteLine("Shift end time cleared.");
                return;
            }

            // Use RealTime to avoid drift issues during long shifts
            var endTime = _timing.RealTime + TimeSpan.FromHours(hours);
            ticker.ShiftEndTime = endTime;
            
            shell.WriteLine($"Shift end time set to {hours} hours from now (server real time: {endTime}).");
        }
    }
}
