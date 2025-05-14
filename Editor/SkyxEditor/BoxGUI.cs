using UnityEngine;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class BoxGUI
    {
        public static void DrawBox(Rect rect, EColor color)
        {
            using var scope = BackgroundColorScope.Set(SkyxStyles.BoxColor(color));
            GUI.Box(rect, GUIContent.none, SkyxStyles.BoxStyle(color));
        }

        public static void ShrinkHeaderRect(ref Rect headerRect, float headerHeight)
        {
            headerRect.height = headerHeight - 2;
            headerRect.y += 1;
            headerRect.x += 1;
            headerRect.width -= 2;
        }
    }
}