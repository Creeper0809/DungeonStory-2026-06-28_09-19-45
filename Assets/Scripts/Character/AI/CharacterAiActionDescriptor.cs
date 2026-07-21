using System;
using System.Collections.Generic;

public static class CharacterAiActionTags
{
    public const string Work = "ai:work";
    public const string Curiosity = "ai:curiosity";
    public const string SelfCare = "ai:self-care";
    public const string Shopping = "ai:shopping";
    public const string Patience = "ai:patience";
    public const string Exit = "ai:exit";
}

public sealed class CharacterAiActionDescriptor
{
    private readonly HashSet<string> semanticTags;
    private readonly IReadOnlyCollection<string> semanticTagsView;

    public CharacterAiActionDescriptor(
        CharacterAiBranch branch,
        string defaultLabel,
        params string[] semanticTags)
    {
        Branch = branch;
        DefaultLabel = defaultLabel ?? string.Empty;
        this.semanticTags = new HashSet<string>(StringComparer.Ordinal);
        semanticTagsView = ReadOnlyView.Collection(this.semanticTags);
        if (semanticTags == null)
        {
            return;
        }

        for (int i = 0; i < semanticTags.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(semanticTags[i]))
            {
                this.semanticTags.Add(semanticTags[i].Trim());
            }
        }
    }

    public static CharacterAiActionDescriptor None { get; } = new CharacterAiActionDescriptor(
        CharacterAiBranch.None,
        string.Empty);

    public CharacterAiBranch Branch { get; }
    public string DefaultLabel { get; }
    public IReadOnlyCollection<string> SemanticTags => semanticTagsView;

    public bool HasTag(string tag)
    {
        return !string.IsNullOrWhiteSpace(tag) && semanticTags.Contains(tag);
    }
}
