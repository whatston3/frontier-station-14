using System.Numerics;
using Content.Server._NF.GameRule;
using Content.Server.Administration;
using Content.Shared._NF.GameRule;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Robust.Server.Console.Commands
{
    // Function to load a shuttle and initialize it as a station.
    // Pretty much duplicates Adventure rule/Shipyard shuttle init.
    [AdminCommand(AdminFlags.Spawn)]
    public sealed class LoadPOICommand : LocalizedCommands
    {
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;

        public override string Command => "loadpoi";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // loadpoi Tinnia X Y [name]
            if (args.Length < 3)
            {
                shell.WriteError(Loc.GetString("cmd-loadpoi-not-enough-args"));
                shell.WriteError(Loc.GetString("cmd-loadpoi-help"));
                return;
            }

            if (!_prototype.TryIndex<PointOfInterestPrototype>(args[0], out var poi))
            {
                shell.WriteError(Loc.GetString("cmd-loadpoi-invalid-poi"));
                return;
            }

            if (!int.TryParse(args[1], out var x))
            {
                shell.WriteError(Loc.GetString("cmd-loadpoi-invalid-x"));
                return;
            }

            if (!int.TryParse(args[2], out var y))
            {
                shell.WriteError(Loc.GetString("cmd-loadpoi-invalid-y"));
                return;
            }

            string? overrideName = null;
            if (args.Length > 3)
            {
                overrideName = args[3];
            }

            var adventure = _entManager.SystemOrNull<NfAdventureRuleSystem>();
            if (adventure == null)
            {
                shell.WriteError(Loc.GetString("cmd-loadpoi-no-gamerule"));
                return;
            }

            if (adventure.TrySpawnPoiGrid(poi, new Vector2(x, y), out var gridUid, overrideName))
            {
                shell.WriteError(Loc.GetString("cmd-loadpoi-success", ("poi", poi.ID), ("x",x), ("y", y), ("gridUid", gridUid?.ToString() ?? "null")));
            }
            else
            {
                shell.WriteError(Loc.GetString("cmd-loadpoi-error", ("poi", poi.ID)));
            }
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    var protos = _prototype.EnumeratePrototypes<PointOfInterestPrototype>();
                    List<string> protoIds = new();
                    foreach (var proto in protos)
                    {
                        protoIds.Add(proto.ID);
                    }
                    return CompletionResult.FromOptions(protoIds);
                case 1:
                    return CompletionResult.FromHint("cmd-hint-loadpoi-x-position");
                case 2:
                    return CompletionResult.FromHint("cmd-hint-loadpoi-y-position");
                case 3:
                    return CompletionResult.FromHint("cmd-hint-loadpoi-name");
            }
            return CompletionResult.Empty;
        }
    }
}
