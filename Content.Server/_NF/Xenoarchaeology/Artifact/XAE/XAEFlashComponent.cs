using Robust.Shared.Audio;

namespace Content.Server._NF.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// Generates a flash from the artifact when activated.
/// </summary>
[RegisterComponent, Access(typeof(XAEFlashSystem))]
public sealed partial class XAEFlashComponent : Component
{
    /// <summary>
    /// The range of the flash, in meters.
    /// </summary>
    [DataField(required: true)]
    public float Range = new();

    /// <summary>
    /// The duration of the flash, in seconds.
    /// </summary>
    [DataField(required: true)]
    public float Duration;

    /// <summary>
    /// The sound to play when the flash triggers.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;
}

