using Robust.Shared.Utility;
using System.Numerics; // Frontier

namespace Content.Shared.Atmos.Components;

[RegisterComponent]
public sealed partial class PipeAppearanceComponent : Component
{
    [DataField("sprite")]
    public SpriteSpecifier.Rsi Sprite = new(new("Structures/Piping/Atmospherics/pipe.rsi"), "pipeConnector");

    // Frontier: pipe offset for cryo tubes
    // [DataField]
    // public Vector2 Offset = Vector2.Zero;
    // End Frontier
}
