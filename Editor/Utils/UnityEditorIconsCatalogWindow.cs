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

        string search = "";
        bool textureName = false;

        [MenuItem("K10/Utils/Editor Icons Catalog")] private static void Init() { GetWindow<UnityEditorIconsCatalogWindow>(false, "Unity Editor Icons Catalog").Show(); }

        private void OnGUI()
        {
            if (icons == null || iconFields == null) 
            {
                iconFields = typeof(UnityIcons).GetFields(BindingFlags.Static | BindingFlags.Public);
                icons = iconFields.Select(x => EditorGUIUtility.Load( x.GetValue(null) as string ) as Texture2D ).ToArray();
            }

            GUILayout.BeginHorizontal();
            search = EditorGUILayout.TextField( search, EditorStyles.toolbarSearchField);
            if( GUILayout.Button( textureName ? "Texture Name" : "Field Name", EditorStyles.miniButton, GUILayout.Width( 120 ) ) ) textureName = !textureName;
            GUILayout.EndHorizontal();

            _scroll = GUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < icons.Length; i++)
            {
                Texture2D icon = icons[i];
                if (icon == null) continue;
                var name = icon.name;
                var field = iconFields[i];
                if( !textureName && field != null ) name = field.Name;
                if( !name.Contains( search ) ) continue;
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(name, GUILayout.Width(250));
                GUILayout.Label(icon);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}