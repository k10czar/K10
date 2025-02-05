using K10.DebugSystem;
using UnityEngine;

public class InputLogCategory : IK10LogCategory
{
    public string Name => "🎮Input";
    public Color Color => Colors.Aquamarine;
}
