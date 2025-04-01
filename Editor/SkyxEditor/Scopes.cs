using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class SimpleBoxScope : GUI.Scope
    {
        public SimpleBoxScope()
        {
            BoxGUI.BeginBox();
        }

        public SimpleBoxScope(string title, float headerHeight = SkyxStyles.BoxHeaderHeight)
        {
            BoxGUI.BeginHeaderBox(new GUIContent(title), headerHeight);
        }

        protected override void CloseScope()
        {
            BoxGUI.EndBox();
        }
    }

    public class ToggleBoxScope : GUI.Scope
    {
        private readonly bool disableContent;

        public ToggleBoxScope(GUIContent title, SerializedProperty toggle, float headerHeight = SkyxStyles.BoxHeaderHeight, bool disableContent = true)
        {
            this.disableContent = disableContent;
            var value = toggle.boolValue;
            toggle.boolValue = BoxGUI.BeginToggle(title, ref value, headerHeight);
            if (disableContent) GUI.enabled = toggle.boolValue;
        }

        protected override void CloseScope()
        {
            if (disableContent) GUI.enabled = true;
            BoxGUI.EndBox();
        }
    }

    public class IconSizeScope : GUI.Scope
    {
        private readonly Vector2 prevIconSize;

        public IconSizeScope(Vector2 iconSize)
        {
            prevIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(iconSize);
        }

        public IconSizeScope(float iconSize)
        {
            prevIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(iconSize, iconSize));
        }

        public IconSizeScope(float x, float y)
        {
            prevIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(x, y));
        }

        protected override void CloseScope()
        {
            EditorGUIUtility.SetIconSize(prevIconSize);
        }
    }

    public class BackgroundColorScope : GUI.Scope
    {
        private readonly Color prevColor;

        public BackgroundColorScope(EColor color)
        {
            prevColor = GUI.backgroundColor;
            GUI.backgroundColor = color.Get();
        }

        public BackgroundColorScope(Color backgroundColor)
        {
            prevColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
        }

        public BackgroundColorScope(string htmlColor)
        {
            prevColor = GUI.backgroundColor;
            if (ColorUtility.TryParseHtmlString(htmlColor, out Color bgColor))
                GUI.backgroundColor = bgColor;
        }

        protected override void CloseScope()
        {
            GUI.backgroundColor = prevColor;
        }
    }
}