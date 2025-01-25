using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Materials;
using Content.Server.Power.Components;
using Content.Shared.Conveyor;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Physics.Controllers;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Physics.Controllers;

public sealed class JostlingDamageController : VirtualController
{
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly MaterialReclaimerSystem _materialReclaimer = default!;
    [Dependency] private readonly SharedBroadphaseSystem _broadphase = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    private const float MinimumVelocityThreshold = 0.01f;
    private DamageSpecifier _damagePerSecond = new DamageSpecifier() { };

    public override void Initialize()
    {
        UpdatesAfter.Add(typeof(ConveyorController));

        SubscribeLocalEvent<FragileWhenJostledComponent, StartCollideEvent>(OnConveyedStartCollide);
        SubscribeLocalEvent<FragileWhenJostledComponent, EndCollideEvent>(OnConveyedEndCollide);

        var damageType = _proto.Index<DamageTypePrototype>("Blunt");
        _damagePerSecond = new DamageSpecifier(damageType, 10);

        base.Initialize();
    }

    private void OnConveyedStartCollide(Entity<FragileWhenJostledComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.Jostling.Contains(args.OtherEntity))
            return;

        ent.Comp.Jostling.Add(args.OtherEntity);
    }

    private void OnConveyedEndCollide(Entity<FragileWhenJostledComponent> ent, ref EndCollideEvent args)
    {
        if (!ent.Comp.Jostling.Remove(args.OtherEntity))
            return;
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var query = EntityQueryEnumerator<FragileWhenJostledComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            var mapCoordinates = _transform.ToMapCoordinates(xform.Coordinates);
            if (comp.Jostling.Count == 0)
            {
                comp.LastPosition = xform.Coordinates;
                comp.LastWorldPosition = mapCoordinates;
                continue;
            }

            if (comp.LastPosition.EntityId == xform.Coordinates.EntityId)
            {
                if ((comp.LastPosition.Position - xform.Coordinates.Position).Length() > MinimumVelocityThreshold * frameTime)
                {
                    _damage.TryChangeDamage(uid, _damagePerSecond * frameTime, ignoreResistances: true);
                }
            }
            else
            {
                if ((comp.LastWorldPosition.Position - mapCoordinates.Position).Length() > MinimumVelocityThreshold * frameTime)
                {
                    _damage.TryChangeDamage(uid, _damagePerSecond * frameTime, ignoreResistances: true);
                }
            }
            comp.LastPosition = xform.Coordinates;
            comp.LastWorldPosition = mapCoordinates;
        }
    }
}
