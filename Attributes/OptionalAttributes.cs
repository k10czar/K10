using UnityEngine;

public class OptionalNumAttribute : PropertyAttribute
{
    public readonly string compact;
    public readonly string hint;
    public readonly bool showLabel;
    public readonly bool useInfinite;

    public OptionalNumAttribute(string compact, string hint, bool useInfinite = false, bool showLabel = true)
    {
        this.compact = compact;
        this.hint = hint;
        this.useInfinite = useInfinite;
        this.showLabel = showLabel;
    }
}

public class OptionalStringAttribute : PropertyAttribute
{
    public readonly string compact;
    public readonly string hint;
    public readonly bool showLabel;
    public readonly string optionalValue;
    public readonly string defaultValue;

    public OptionalStringAttribute(string compact, string hint, string defaultValue = "NonOptional", string optionalValue = "", bool showLabel = true)
    {
        this.compact = compact;
        this.hint = hint;
        this.optionalValue = optionalValue;
        this.defaultValue = defaultValue;
        this.showLabel = showLabel;
    }
}

public class OptionalAttribute : PropertyAttribute
{
    public readonly string compact;
    public readonly string hint;

    public OptionalAttribute(string compact, string hint = null)
    {
        this.compact = compact;
        this.hint = hint;
    }
}