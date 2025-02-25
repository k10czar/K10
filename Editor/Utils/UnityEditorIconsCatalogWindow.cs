using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace K10.EditorUtils
{
    public class UnityEditorIconsCatalogWindow : EditorWindow
    {
        // private FieldInfo[] iconNames;
        private Texture2D[] icons;
        private Vector2 _scroll;

        bool publicExpanded = true;
        bool nonPublicExpanded = false;

        [MenuItem("K10/Utils/Editor Icons Catalog")] private static void Init() { GetWindow<UnityEditorIconsCatalogWindow>(false, "Unity Editor Icons Catalog").Show(); }

        private void OnGUI()
        {
            if (icons == null) 
            {
                var iconNames = typeof(UnityIcons).GetFields(BindingFlags.Static | BindingFlags.Public);
                icons = iconNames.Select(x => EditorGUIUtility.Load( x.GetValue(null) as string ) as Texture2D ).ToArray();
            }

            _scroll = GUILayout.BeginScrollView(_scroll);

            foreach (var icon in icons)
            {
                if (icon == null) continue;
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField($"{icon.name}", GUILayout.Width(150));
                GUILayout.Label(icon);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}