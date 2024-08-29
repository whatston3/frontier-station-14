using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.EntityEffects.Effects
{
    [UsedImplicitly]
    public sealed partial class LowYieldWhenImpreciseReaction : EntityEffect
    {
        [DataField(required: true)]
        public Dictionary<string, float> AffectedReagentRatios;
        // Placeholder: Proof of concept for random reagent application.
        private readonly string[] _junkReagents = { "VentCrud", "GastroToxin", "Allicin", "Tazinide", "Histamine", "Mold" };

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is EntityEffectReagentArgs reagentArgs)
            {
                var random = IoCManager.Resolve<IRobustRandom>();
                if (random == null)
                    return;

                // TODO see if this is correct
                var solutionContainerSystem = reagentArgs.EntityManager.System<SharedSolutionContainerSystem>();
                if (solutionContainerSystem == null || reagentArgs.Source == null)
                    return;

                foreach (var (reagent, ratio) in AffectedReagentRatios)
                {
                    var removedAmount = reagentArgs.Source.RemoveReagent(reagent, reagentArgs.Quantity * ratio * 0.3f);
                    if (removedAmount > 0)
                        reagentArgs.Source.AddReagent(random.Pick(_junkReagents), removedAmount);
                }
                return;
            }

            // TODO: Someone needs to figure out how to do this for non-reagent effects.
            throw new NotImplementedException();
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
            Loc.GetString("reagent-effect-guidebook-low-yield-when-imprecise-reaction", ("chance", Probability));
    }
}