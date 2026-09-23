using Rogue.REditor;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.Explorer
{
    [CustomPropertyDrawer(typeof(ExplorerBatchExecute<>), true)]
    public class ExplorerBatchExecuteDrawer : CustomSkopedDrawer
    {
        protected override ScopedAttribute CreateScopedAttribute()
        {
            var att = base.CreateScopedAttribute();
            att.hasCustomExpand = true;

            return att;
        }

        protected override void DrawContent(ref Rect rect, SerializedProperty property, SkopeInfo info)
        {
            base.DrawContent(ref rect, property, info);

            SkyxGUI.Separator(ref rect);

            if (SkyxGUI.Button(rect, "Run", EColor.Warning))
            {
                var batchBase = property.GetValue<ExplorerBatchExecuteBase>();
                var configBase = property.GetParentValue<ExplorerSearchConfigBase>(true);

                batchBase.Run(configBase);
            }
        }

        protected override float GetContentHeight(SerializedProperty property, SkopeInfo info)
            => base.GetContentHeight(property, info) + SkyxStyles.FullSeparatorSize + SkyxStyles.FullLineHeight;
    }
}