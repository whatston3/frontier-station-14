using Content.Server.Chemistry.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Server._NF.Chemistry
{

    /// <summary>
    /// Contains all the server-side logic for ChemMasters.
    /// <seealso cref="ChemMasterComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class NFPrecisionMachineSystem : EntitySystem
    {
        [Dependency] private SharedSolutionContainerSystem _solution = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<NFPrecisionMachineComponent, EntInsertedIntoContainerMessage>(ItemAdded);
            SubscribeLocalEvent<NFPrecisionMachineComponent, EntRemovedFromContainerMessage>(ItemRemoved);
        }

        private void ItemAdded(Entity<NFPrecisionMachineComponent> ent, ref EntInsertedIntoContainerMessage ev)
        {
            if (_solution.TryGetFitsInDispenser((ev.Entity, null, null), out var soln, out var _))
            {
                EnsureComp<NFPreciseReactionComponent>(soln.Value.Owner);
            }
        }

        private void ItemRemoved(Entity<NFPrecisionMachineComponent> ent, ref EntRemovedFromContainerMessage ev)
        {
            if (_solution.TryGetFitsInDispenser((ev.Entity, null, null), out var soln, out var _))
            {
                RemComp<NFPreciseReactionComponent>(soln.Value.Owner);
            }
        }
    }
}
