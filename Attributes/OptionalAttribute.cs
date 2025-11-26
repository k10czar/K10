using UnityEngine;

public class OptionalAttribute : PropertyAttribute
{
    public readonly string compact;
    public readonly string hint;
    public readonly bool showLabel;

    public OptionalAttribute(string compact, string hint, bool showLabel = true)
    {
        this.compact = compact;
        this.hint = hint;
        this.showLabel = showLabel;
    }
}