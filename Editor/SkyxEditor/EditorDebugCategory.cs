using System.Collections.Generic;
using K10.DebugSystem;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class EditorDebugCategory : IDebugCategory
    {
        public string Name => "Editor";
        public Color Color => Colors.Beige;

        public List<GameObject> HiddenObjects { get; } = new();
    }
}