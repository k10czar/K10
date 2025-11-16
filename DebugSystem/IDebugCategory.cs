using System;
using System.Collections.Generic;
using UnityEngine;

namespace K10.DebugSystem
{
    public interface IDebugCategory
    {
        string Name { get; }
        Color Color { get; }
        Color SecondaryColor => Color.AddLight(-.1f);

        DebugFlag[] Flags => Array.Empty<DebugFlag>();
        List<GameObject> HiddenObjects => null;
    }

    public class TempDebug : IDebugCategory
    {
        public string Name => "Temp";
        public Color Color => Colors.Orange;
    }

    public class EditorDebug : IDebugCategory
    {
        public string Name => "Editor";
        public Color Color => Colors.Beige;

        public List<GameObject> HiddenObjects { get; } = new();
    }
}