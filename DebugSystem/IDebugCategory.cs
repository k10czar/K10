using UnityEngine;

namespace K10.DebugSystem
{
    public interface IDebugCategory
    {
        string Name { get; }
        Color Color { get; }
        string[] CustomFlags => null;
        Color SecondaryColor => Color.AddLight(-.1f);
    }

    public class TempDebugCategory : IDebugCategory
    {
        public string Name => "Temp";
        public Color Color => Colors.Orange;
    }
}