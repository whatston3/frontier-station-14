using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Tag;

/// <summary>
/// A component for adding or removing tags through inheritance.
/// Intended to prevent breaking issues from tags being added.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(NFTagSystem))]
public sealed partial class NFTagComponent : Component
{
    /// <summary>
    /// A list of tags to add.
    /// Note: this inherits its parents' tags!
    /// If AddTagOverride is specified, none of these are used.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public List<ProtoId<TagPrototype>> AddTags = new();

    /// <summary>
    /// If specified, adds a specific set of tags to THIS ENTITY ONLY, and not its children.
    /// </summary>
    [DataField, NeverPushInheritance]
    public List<ProtoId<TagPrototype>>? AddTagsOverride = null;

    /// <summary>
    /// A list of tags to add.
    /// Note: this inherits its parents' tags!
    /// If AddTagOverride is specified, none of these are used.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public List<ProtoId<TagPrototype>> RemoveTags = new();

    /// <summary>
    /// If specified, removes a specific set of tags from THIS ENTITY ONLY, and not its children.
    /// </summary>
    [DataField, NeverPushInheritance]
    public List<ProtoId<TagPrototype>>? RemoveTagsOverride = null;
}
