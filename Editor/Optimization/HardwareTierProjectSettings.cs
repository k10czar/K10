using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace K10.Optimization.Editor
{
    // Native Unity Project Settings storage: serialized to the versioned ProjectSettings/ folder
    // (not as an asset under Assets/). Editor-only. The values here are baked into player builds by
    // HardwareTierBakedCodeGenerator, so the generic runtime library (HardwareTier) stays untouched.
    [FilePath ("ProjectSettings/HardwareTier.asset", FilePathAttribute.Location.ProjectFolder)]
    public class HardwareTierProjectSettings : ScriptableSingleton<HardwareTierProjectSettings>
    {
        [Serializable]
        public class PlatformOverride
        {
            [Tooltip ("Runtime platform this override applies to (matched against Application.platform).")]
            public RuntimePlatform platform = RuntimePlatform.Android;

            public MemoryTierThresholds memoryTiers = new MemoryTierThresholds ();
        }

        [Tooltip ("Bounds used when no platform override matches. Platforms without an override fall back to these.")]
        [SerializeField] internal MemoryTierThresholds _defaultMemoryTiers = new MemoryTierThresholds ();

        [Tooltip ("Optional per-platform overrides. When the running platform matches, its bounds are used " +
            "instead of the default.")]
        [SerializeField] internal List<PlatformOverride> _platformOverrides = new List<PlatformOverride> ();

        public float[] DefaultMemoryTiers => _defaultMemoryTiers.ToArray ();
        public List<PlatformOverride> PlatformOverrides => _platformOverrides;

        public void Persist () => Save (true);
    }
}
