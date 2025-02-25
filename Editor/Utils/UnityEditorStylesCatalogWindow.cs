using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace K10.EditorUtils
{
    public class UnityEditorStylesCatalogWindow : EditorWindow
    {
        private PropertyInfo[] publicProperties;
        private PropertyInfo[] nonPublicProperties;
        private Vector2 _scroll;

        bool publicExpanded = true;
        bool nonPublicExpanded = false;

        [MenuItem("K10/Utils/EditorStylesCatalog")] private static void Init() { GetWindow<UnityEditorStylesCatalogWindow>(false, "Unity EditorStyles Catalog").Show(); }

        private void OnGUI()
        {
            if (publicProperties == null)
            {
                publicProperties = typeof(EditorStyles).GetProperties(BindingFlags.Static | BindingFlags.Public);
                publicProperties.Where(p => p.PropertyType == typeof(GUIStyle));
            }

            _scroll = GUILayout.BeginScrollView(_scroll);

            publicExpanded = EditorGUILayout.BeginFoldoutHeaderGroup( publicExpanded, $"Public Styles");
            if( publicExpanded ) ListCatalog( publicProperties );
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (nonPublicProperties == null)
            {
                nonPublicProperties = typeof(EditorStyles).GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
                nonPublicProperties.Where(p => p.PropertyType == typeof(GUIStyle));
            }

            nonPublicExpanded = EditorGUILayout.BeginFoldoutHeaderGroup( nonPublicExpanded, $"Non Public Styles");
            if( nonPublicExpanded ) ListCatalog( nonPublicProperties );
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.EndScrollView();
        }

        private void ListCatalog(IEnumerable<PropertyInfo> props)
        {
            foreach (var prop in props)
            {
                if (prop == null) continue;
                var sytle = prop.GetValue(null) as GUIStyle;
                if (sytle == null) continue;
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField($"{prop.Name}", GUILayout.Width(150));
                GUILayout.Label($"{prop.Name}({prop.PropertyType})", sytle);
                GUILayout.EndHorizontal();
            }
        }
    }
}