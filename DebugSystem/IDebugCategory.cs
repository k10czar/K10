using System;
using UnityEngine;

namespace K10.DebugSystem
{
    public interface IDebugCategory
    {
        string Name { get; }
        Color Color { get; }
        Color SecondaryColor => Color.AddLight(-.1f);

        DebugFlag[] Flags => Array.Empty<DebugFlag>();
    }

    public class TempDebugCategory : IDebugCategory
    {
        public string Name => "Temp";
        public Color Color => Colors.Orange;
    }
}