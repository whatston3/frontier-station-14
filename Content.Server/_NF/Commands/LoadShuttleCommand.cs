using System.Numerics;
using Content.Server.Administration;
using Content.Server.Shipyard.Systems;
using Content.Shared.Administration;
using Content.Shared.Shipyard.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Robust.Server.Console.Commands
{
    // Function to load a shuttle and initialize it as a station.
    // Pretty much duplicates Adventure rule/Shipyard shuttle init.
    [AdminCommand(AdminFlags.Spawn)]
    public sealed class LoadVesselCommand : LocalizedCommands
    {
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;

        public override string Command => "loadvessel";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // loadpoi Tinnia X Y [name]
            if (args.Length < 3)
            {
                shell.WriteError(Loc.GetString("cmd-loadvessel-not-enough-args"));
                return;
            }

            if (!_prototype.TryIndex<VesselPrototype>(args[0], out var vessel))
            {
                shell.WriteError(Loc.GetString("cmd-loadvessel-invalid-vessel"));
                return;
            }

            if (!int.TryParse(args[1], out var x))
            {
                shell.WriteError(Loc.GetString("cmd-loadvessel-invalid-x"));
                return;
            }

            if (!int.TryParse(args[2], out var y))
            {
                shell.WriteError(Loc.GetString("cmd-loadvessel-invalid-y"));
                return;
            }

            string? overrideName = null;
            if (args.Length > 3)
            {
                overrideName = args[3];
            }

            var shipyard = _entManager.SystemOrNull<ShipyardSystem>();
            if (shipyard == null)
            {
                shell.WriteError(Loc.GetString("cmd-loadvessel-no-system"));
                return;
            }

            if (shipyard.TryAddShuttle(vessel.ShuttlePath, out var gridUid, new Vector2(x, y)))
            {
                if (overrideName != null)
                {
                    shipyard.RenameShuttle(gridUid, null, overrideName);
                }
                shell.WriteError(Loc.GetString("cmd-loadvessel-gen-success", ("poi", poi.ID), ("x",x), ("y", y), ("gridUid", gridUid?.ToString() ?? "null")));
            }
            else
            {
                shell.WriteError(Loc.GetString("cmd-loadvessel-gen-failure", ("poi", poi.ID)));
            }
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            switch (args.Length)
            {
                case 1:
                    var protos = _prototype.EnumeratePrototypes<VesselPrototype>();
                    List<string> protoIds = new();
                    foreach (var proto in protos)
                    {
                        protoIds.Add(proto.ID);
                    }
                    return CompletionResult.FromOptions(protoIds);
                case 2:
                    return CompletionResult.FromHint("cmd-hint-loadvessel-x-position");
                case 3:
                    return CompletionResult.FromHint("cmd-hint-loadvessel-y-position");
                case 4:
                    return CompletionResult.FromHint("cmd-hint-loadvessel-name");
            }
            return CompletionResult.Empty;
        }
    }
}
