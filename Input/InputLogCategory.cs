using K10.DebugSystem;
using UnityEngine;

public class InputLogCategory : IDebugCategory
{
    public string Name => "🎮Input";
    public Color Color => Colors.Aquamarine;
}