using Content.Shared.Examine;
using Content.Shared._NF.Construction.Components;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Content.Shared.Tools;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics;
using Content.Shared.Interaction;
using Content.Shared.Tools.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Content.Shared.Tools.Systems;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._NF.Construction.EntitySystems;

public sealed partial class RandomAnchoredSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomAnchoredComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RandomAnchoredComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RandomAnchoredComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<RandomAnchoredComponent, TryRemoveRandomAnchoredEvent>(OnRemoveRandomAnchored);
    }

    private void OnMapInit(Entity<RandomAnchoredComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.PossibleToolUses.Count <= 0
            || !TryComp(ent, out PhysicsComponent? phys)
            || phys.BodyType != BodyType.Dynamic
            || !_random.Prob(ent.Comp.AnchoredProb))
        {
            RemComp<RandomAnchoredComponent>(ent);
            return;
        }

        // TODO: pick random tool
        ProtoId<ToolQualityPrototype> selectedToolUse = _random.Pick(ent.Comp.PossibleToolUses);
        _physics.SetBodyType(ent, BodyType.Static, body: phys);
        ent.Comp.SelectedToolUse = selectedToolUse;
    }

    private void OnExamine(Entity<RandomAnchoredComponent> ent, ref ExaminedEvent args)
    {
        if (TryComp(ent, out PhysicsComponent? phys) && phys.BodyType == BodyType.Static)
        {
            using (args.PushGroup(nameof(RandomAnchoredSystem)))
            {
                args.PushMarkup(Loc.GetString("random-anchored-stuck"));
                args.PushMarkup(Loc.GetString("random-anchored-examine", ("tool", ent.Comp.SelectedToolUse)));
            }
        }
    }

    private void OnInteractUsing(Entity<RandomAnchoredComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // If the used entity doesn't have a tool, return early.
        if (!TryComp(args.Used, out ToolComponent? usedTool) || !_tool.HasQuality(args.Used, ent.Comp.SelectedToolUse, usedTool))
            return;

        if (!TryComp(ent, out PhysicsComponent? phys) || phys.BodyType != BodyType.Static)
            return;

        if (!TryComp(ent, out TransformComponent? xform))
            return;

        args.Handled = true;

        // Log unanchor attempt (server only)
        _adminLogger.Add(LogType.Anchor, LogImpact.Low, $"{ToPrettyString(args.User):user} is trying to unanchor {ToPrettyString(ent):entity} from {xform.Coordinates:targetlocation}");

        _tool.UseTool(args.Used, args.User, ent, ent.Comp.CurrentDelay, ent.Comp.SelectedToolUse, new TryRemoveRandomAnchoredEvent()); // Frontier: Delay<CurrentDelay
    }

    private void OnRemoveRandomAnchored(Entity<RandomAnchoredComponent> ent, ref TryRemoveRandomAnchoredEvent args)
    {
        if (!TryComp(args.Used, out ToolComponent? usedTool) || !_tool.HasQuality(args.Used.Value, ent.Comp.SelectedToolUse, usedTool))
            return;

        if (TryComp(ent, out PhysicsComponent? phys) && phys.BodyType != BodyType.Static)
            return;

        _physics.SetBodyType(ent, BodyType.Dynamic, body: phys);
        RemComp<RandomAnchoredComponent>(ent);
    }

    [Serializable, NetSerializable]
    private sealed partial class TryRemoveRandomAnchoredEvent : SimpleDoAfterEvent
    {
    }
}
