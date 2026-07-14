using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace K10.Optimization.Editor
{
    // Surfaces the hardware tier config as a native Project Settings category (Edit > Project Settings > K10).
    static class HardwareTierSettingsProvider
    {
        const float IconWidth = 20f;
        const float Pad = 4f;

        static readonly Dictionary<string, GUIContent> platformIconCache = new Dictionary<string, GUIContent> ();

        [SettingsProvider]
        public static SettingsProvider Create ()
        {
            // Editing is cheap (ApplyModifiedProperties keeps the in-memory object live), but persisting to
            // ProjectSettings/ and regenerating the baked .cs (which recompiles) is slow, so it is deferred to
            // the Apply button or to leaving the page instead of running on every keystroke.
            SerializedObject serialized = null;
            ReorderableList overridesList = null;
            bool dirty = false;

            void EnsureBound ()
            {
                if (serialized != null && serialized.targetObject != null) return;

                serialized = new SerializedObject (HardwareTierProjectSettings.instance);
                overridesList = BuildOverridesList (serialized, () => dirty = true);
            }

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
                activateHandler = (_, __) => { EnsureBound (); HardwareTierBakedCodeGenerator.Regenerate (); },

                // Persist + regenerate when the user navigates away without pressing Apply.
                deactivateHandler = Commit,

                guiHandler = _ =>
                {
                    EnsureBound ();
                    serialized.Update ();

                    EditorGUILayout.HelpBox (
                        "Memory (GB) upper bounds for the Low / Mid / High tiers. Anything above the High bound is Extreme. " +
                        "The running platform uses its override if one exists, otherwise the default. " +
                        "These values are baked into player builds; the generic HardwareTier library keeps its defaults if unset.",
                        MessageType.Info);

                    EditorGUI.BeginChangeCheck ();

                    EditorGUILayout.PropertyField (serialized.FindProperty ("_defaultMemoryTiers"), new GUIContent ("Default Memory Tiers"), true);
                    EditorGUILayout.Space ();
                    overridesList.DoLayoutList ();

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

        static ReorderableList BuildOverridesList (SerializedObject serialized, System.Action markDirty)
        {
            SerializedProperty prop = serialized.FindProperty ("_platformOverrides");
            var emptyLabel = new GUIContent (" ");

            ReorderableList list = new ReorderableList (serialized, prop, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField (rect, "Platform Overrides"),

                elementHeightCallback = index =>
                {
                    SerializedProperty tiers = prop.GetArrayElementAtIndex (index).FindPropertyRelative ("memoryTiers");
                    return EditorGUI.GetPropertyHeight (tiers) + Pad;
                },

                drawElementCallback = (rect, index, active, focused) =>
                {
                    SerializedProperty element = prop.GetArrayElementAtIndex (index);
                    SerializedProperty platform = element.FindPropertyRelative ("platform");
                    SerializedProperty tiers = element.FindPropertyRelative ("memoryTiers");

                    float h = EditorGUIUtility.singleLineHeight;
                    float y = rect.y + Pad * 0.5f;

                    // Draw the tiers (with reachable-tier preview) using an empty label; its first row still
                    // reserves the label column, which we reuse for the platform icon + dropdown.
                    Rect tiersRect = new Rect (rect.x, y, rect.width, EditorGUI.GetPropertyHeight (tiers));
                    EditorGUI.PropertyField (tiersRect, tiers, emptyLabel, true);

                    // Overlay platform icon + dropdown on top of that reserved label column, saving a line.
                    float px = rect.x;
                    GUIContent icon = PlatformIcon (PlatformName (platform));
                    if (icon != null)
                    {
                        GUI.Label (new Rect (px, y, IconWidth, h), icon);
                        px += IconWidth + Pad;
                    }

                    float labelRight = rect.x + EditorGUIUtility.labelWidth;
                    EditorGUI.PropertyField (new Rect (px, y, Mathf.Max (0f, labelRight - px), h), platform, GUIContent.none);
                },

                onAddCallback = reorderable =>
                {
                    int index = prop.arraySize;
                    prop.arraySize++;

                    SerializedProperty element = prop.GetArrayElementAtIndex (index);
                    SerializedProperty tiers = element.FindPropertyRelative ("memoryTiers");
                    tiers.FindPropertyRelative ("low").floatValue = 6f;
                    tiers.FindPropertyRelative ("mid").floatValue = 0f;
                    tiers.FindPropertyRelative ("high").floatValue = 0f;

                    reorderable.index = index;
                },

                onChangedCallback = _ =>
                {
                    serialized.ApplyModifiedProperties ();
                    markDirty ();
                },
            };

            return list;
        }

        static string PlatformName (SerializedProperty platform)
        {
            int i = platform.enumValueIndex;
            string[] names = platform.enumNames;
            return i >= 0 && i < names.Length ? names[i] : string.Empty;
        }

        static GUIContent PlatformIcon (string platformName)
        {
            if (platformIconCache.TryGetValue (platformName, out GUIContent cached))
                return cached;

            GUIContent content = LoadIcon (PlatformIconBaseName (platformName));
            platformIconCache[platformName] = content;
            return content;
        }

        static string PlatformIconBaseName (string platformName)
        {
            switch (platformName)
            {
                case "Android": return "BuildSettings.Android.Small";
                case "IPhonePlayer": return "BuildSettings.iPhone.Small";
                case "TvOS": return "BuildSettings.tvOS.Small";
                case "WindowsPlayer": case "WindowsServer":
                case "OSXPlayer": case "OSXServer":
                case "LinuxPlayer": case "LinuxServer":
                    return "BuildSettings.Standalone.Small";
                case "PS4": return "BuildSettings.PS4.Small";
                case "PS5": return "BuildSettings.PS5.Small";
                case "XboxOne": return "BuildSettings.XboxOne.Small";
                case "GameCoreXboxSeries": case "GameCoreXboxOne": return "BuildSettings.GameCoreXboxSeries.Small";
                case "Switch": return "BuildSettings.Switch.Small";
                case "WebGLPlayer": return "BuildSettings.WebGL.Small";
                case "WSAPlayerX86": case "WSAPlayerX64": case "WSAPlayerARM": return "BuildSettings.Metro.Small";
                default: return null;
            }
        }

        // Loads a build-platform icon, tolerating skin variants and missing names (falls back to the generic
        // standalone icon, then to no glyph) so the list never throws over an icon lookup.
        static GUIContent LoadIcon (string baseName)
        {
            if (string.IsNullOrEmpty (baseName)) return null;

            GUIContent content = EditorGUIUtility.IconContent (baseName);
            if (content == null || content.image == null)
                content = EditorGUIUtility.IconContent ("d_" + baseName);
            if (content == null || content.image == null)
                content = EditorGUIUtility.IconContent ("BuildSettings.Standalone.Small");

            return content != null && content.image != null ? new GUIContent (content.image) : null;
        }
    }
}
