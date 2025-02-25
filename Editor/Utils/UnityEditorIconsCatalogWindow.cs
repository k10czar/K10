using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace K10.EditorUtils
{
    public class UnityEditorIconsCatalogWindow : EditorWindow
    {
        private FieldInfo[] iconFields;
        private Texture2D[] icons;
        private Vector2 _scroll;

        [MenuItem("K10/Utils/Editor Icons Catalog")] private static void Init() { GetWindow<UnityEditorIconsCatalogWindow>(false, "Unity Editor Icons Catalog").Show(); }

        private void OnGUI()
        {
            if (icons == null || iconFields == null) 
            {
                iconFields = typeof(UnityIcons).GetFields(BindingFlags.Static | BindingFlags.Public);
                icons = iconFields.Select(x => EditorGUIUtility.Load( x.GetValue(null) as string ) as Texture2D ).ToArray();
            }

            _scroll = GUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < icons.Length; i++)
            {
                Texture2D icon = icons[i];
                var field = iconFields[i];
                if (icon == null) continue;
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField($"{field.Name}", GUILayout.Width(250));
                GUILayout.Label(icon);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}