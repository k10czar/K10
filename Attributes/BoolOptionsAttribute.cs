using UnityEngine;

public class BoolOptionsAttribute : PropertyAttribute
{
    private readonly string enabledText;
    private readonly string disabledText;
    private readonly EColor enabledColor;
    private readonly EColor disabledColor;

    private readonly string label;

    public BoolOptionsAttribute(string label, string enabledText, string disabledText, bool isChoice = true)
        : this(enabledText, disabledText, isChoice)
    {
        this.label = label;
    }

    public BoolOptionsAttribute(string enabledText, string disabledText, bool isChoice = true)
    {
        this.enabledText = enabledText;
        this.disabledText = disabledText;
        this.enabledColor = isChoice ? EColor.Special : EColor.Success;
        this.disabledColor = isChoice ? EColor.Info : EColor.Support;
    }

    public BoolOptionsAttribute(string enabledText, string disabledText, EColor enabledColor, EColor disabledColor)
    {
        this.enabledText = enabledText;
        this.disabledText = disabledText;
        this.enabledColor = enabledColor;
        this.disabledColor = disabledColor;
    }

    public string GetLabel(string propertyName) => string.IsNullOrEmpty(label) ? propertyName : label;

    public (string text, EColor color) Get(bool isEnabled)
        => isEnabled ? (enabledText, enabledColor) : (disabledText, disabledColor);
}