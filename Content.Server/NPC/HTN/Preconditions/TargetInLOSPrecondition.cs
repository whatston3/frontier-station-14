using Content.Server.Interaction;
using Content.Shared.Physics;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.NPC.HTN.Preconditions;

public sealed partial class TargetInLOSPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private ISawmill _sawmill = default!;
    private InteractionSystem _interaction = default!;

    [DataField("targetKey")]
    public string TargetKey = "Target";

    [DataField("rangeKey")]
    public string RangeKey = "RangeKey";

    [DataField("collisionMaskKey")] // Frontier: configurable LOS mask
    public string CollisionMaskKey = "CollisionMask"; // Frontier: configurable LOS mask

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _interaction = sysManager.GetEntitySystem<InteractionSystem>();
        _sawmill = Logger.GetSawmill("los_precond");
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager))
        {
            _sawmill.Error("TargetInLOSPrecond: No Target!");
            return false;
        }

        var range = blackboard.GetValueOrDefault<float>(RangeKey, _entManager);

        var collisionMask = (int)blackboard.GetValueOrDefault<float>(CollisionMaskKey, _entManager); // Frontier: check for collision mask

        var ret = _interaction.InRangeUnobstructed(owner, target, range, collisionMask: (CollisionGroup) collisionMask); // Frontier: add mask

        _sawmill.Error($"TargetInLOSPrecond: IsMet {ret} {collisionMask}!");
        return ret;
    }
}
