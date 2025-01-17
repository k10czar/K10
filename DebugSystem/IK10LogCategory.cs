using UnityEngine;

namespace K10.DebugSystem
{
    public interface IK10LogCategory
    {
        string Name { get; }
        Color Color { get; }
        Color SecondaryColor => Color.AddLight(-.1f);
        ELogPrefix PrefixType => ELogPrefix.None;
    }

    public class TempLogCategory : IK10LogCategory
    {
        public string Name => "Temp";
        public Color Color => Colors.Orange;
    }
}