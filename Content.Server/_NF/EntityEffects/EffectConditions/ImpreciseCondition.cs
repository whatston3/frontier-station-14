using Content.Server._NF.Chemistry;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._NF.EntityEffects.EffectConditions;

public sealed partial class ImpreciseCondition : EntityEffectCondition
{
    [DataField] public bool Inverted; // If true, this effect _requires_ precision.

    public override bool Condition(EntityEffectBaseArgs args)
    {
        return args.EntityManager.HasComponent<NFPreciseReactionComponent>(args.TargetEntity) == Inverted;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-imprecise-condition", ("precise", Inverted));
    }
}


