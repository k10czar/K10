using K10.DebugSystem;
using UnityEngine;

public class InputDebug : DebugCategory
{
    public override string Name => "Input";
    public override Color Color => Colors.Aquamarine;

    public override DebugFlag[] Flags { get; } = { ClearConnectionOnRetry };

    public static readonly DebugFlag ClearConnectionOnRetry = new("ClearConnectionOnRetry");
}