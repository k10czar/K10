using UnityEditor;
using UnityEngine;

namespace K10.Optimization.Editor
{
    // Surfaces the hardware tier config as a native Project Settings category (Edit > Project Settings > K10).
    static class HardwareTierSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create ()
        {
            // Editing is cheap (ApplyModifiedProperties keeps the in-memory object live), but persisting to
            // ProjectSettings/ and regenerating the baked .cs (which recompiles) is slow, so it is deferred to
            // the Apply button or to leaving the page instead of running on every keystroke.
            bool dirty = false;

            void Commit ()
            {
                if (!dirty) return;
                HardwareTierProjectSettings.instance.Persist ();
                HardwareTierBakedCodeGenerator.Regenerate ();
                dirty = false;
            }

            return new SettingsProvider ("Project/K10/Hardware Tier", SettingsScope.Project)
            {
                label = "Hardware Tier",
                keywords = new[] { "hardware", "tier", "memory", "quality", "performance", "k10", "optimization" },

                // Make sure the baked runtime file exists / is current whenever the page is opened.
                activateHandler = (_, __) => HardwareTierBakedCodeGenerator.Regenerate (),

                // Persist + regenerate when the user navigates away without pressing Apply.
                deactivateHandler = Commit,

                guiHandler = _ =>
                {
                    HardwareTierProjectSettings settings = HardwareTierProjectSettings.instance;
                    SerializedObject serialized = new SerializedObject (settings);

                    EditorGUILayout.HelpBox (
                        "Memory (MB) upper bounds for the Low / Mid / High tiers. Anything above the High bound is Extreme. " +
                        "The running platform uses its override if one exists, otherwise the default. " +
                        "These values are baked into player builds; the generic HardwareTier library keeps its defaults if unset.",
                        MessageType.Info);

                    EditorGUI.BeginChangeCheck ();
                    EditorGUILayout.PropertyField (serialized.FindProperty ("_defaultMemoryTiers"), new GUIContent ("Default Memory Tiers"), true);
                    EditorGUILayout.Space ();
                    EditorGUILayout.PropertyField (serialized.FindProperty ("_platformOverrides"), new GUIContent ("Platform Overrides"), true);

                    if (EditorGUI.EndChangeCheck ())
                    {
                        // Cheap: only updates the in-memory object. The slow work happens in Commit.
                        serialized.ApplyModifiedProperties ();
                        dirty = true;
                    }

                    EditorGUILayout.Space ();
                    using (new EditorGUI.DisabledScope (!dirty))
                        if (GUILayout.Button (dirty ? "Apply*" : "Apply"))
                            Commit ();
                },
            };
        }
    }
}
