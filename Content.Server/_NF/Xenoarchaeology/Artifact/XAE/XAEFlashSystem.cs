using Content.Server._NF.Xenoarchaeology.Artifact.XAE.Components;
using Content.Server.Flash;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.XAE;

namespace Content.Server._NF.Xenoarchaeology.Artifact.XAE;

/// <summary>
/// System for xeno artifact effect that starts Foam chemical reaction with random-ish reagents inside.
/// </summary>
public sealed class XAEFlashSystem : BaseXAESystem<XAEFlashComponent>
{
    [Dependency] private readonly FlashSystem _flash = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <inheritdoc />
    protected override void OnActivated(Entity<XAEFlashComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        _flash.FlashArea(ent.Owner, ent, ent.Comp.Range, ent.Comp.Duration, sound: ent.Comp.Sound);
    }
}
