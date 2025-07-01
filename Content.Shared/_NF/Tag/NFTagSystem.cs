using Content.Shared.Tag;

namespace Content.Shared._NF.Tag;

public sealed class NFTagSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NFTagComponent, ComponentInit>(OnTagInit, after: [typeof(TagSystem)]);
    }

    private void OnTagInit(Entity<NFTagComponent> ent, ref ComponentInit args)
    {
        if (ent.Comp.AddTagsOverride != null)
        {
            foreach (var tag in ent.Comp.AddTagsOverride)
                _tag.TryAddTag(ent, tag);
        }
        else
        {
            foreach (var tag in ent.Comp.AddTags)
                _tag.TryAddTag(ent, tag);
        }

        if (ent.Comp.RemoveTagsOverride != null)
        {
            foreach (var tag in ent.Comp.RemoveTagsOverride)
                _tag.RemoveTag(ent, tag);
        }
        else
        {
            foreach (var tag in ent.Comp.RemoveTags)
                _tag.RemoveTag(ent, tag);
        }
    }
}
