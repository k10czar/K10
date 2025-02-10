using K10.DebugSystem;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class EditorLogCategory : IK10LogCategory
    {
        public string Name => "🔨Editor";
        public Color Color => Colors.Beige;
    }
}