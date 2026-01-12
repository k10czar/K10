using System;
using System.Collections.Generic;
using UnityEngine;

namespace K10.DebugSystem
{
    public abstract class DebugCategory
    {
        public abstract string Name { get; }
        public abstract Color Color { get; }
        public virtual Color SecondaryColor => Color.AddLight(-.1f);

        public Action<DebugFlag> flagsChanged;
        public Action<EDebugType> changed;

        public virtual DebugFlag[] Flags { get; } = Array.Empty<DebugFlag>();
        public List<GameObject> HiddenObjects { get; set; }

        public void Setup()
        {
            foreach (var flag in Flags)
                flag.SetOwner(this);
        }

        public void Clear()
        {
            HiddenObjects?.Clear();
            flagsChanged = null;
            changed = null;
        }
    }

    public class TempDebug : DebugCategory
    {
        public override string Name => "Temp";
        public override Color Color => Colors.Orange;
    }

    public class EditorDebug : DebugCategory
    {
        public override string Name => "Editor";
        public override Color Color => Colors.Beige;
    }
}