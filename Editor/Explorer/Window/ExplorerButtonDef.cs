using System;
using Skyx.RuntimeEditor;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    public class ExplorerButtonDef : EditorButtonDef<ExplorerEntryDef>
    {
        private Button boundButton;
        private VisualElement boundHolder;
        private bool disabledIsInvisible;

        private string activeStyle;

        public void BindVisuals(Button buttonRef, VisualElement holder, bool disabledIsInvis)
        {
            boundButton = buttonRef;
            boundHolder = holder ?? buttonRef;
            this.disabledIsInvisible = disabledIsInvis;

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            boundButton.text = label;
            boundButton.tooltip = tooltip;

            var newStyle = color.GetUSSStyle();
            if (activeStyle != newStyle)
            {
                if (activeStyle != null) boundHolder.RemoveFromClassList(activeStyle);

                activeStyle = newStyle;
                boundHolder.AddToClassList(activeStyle);
            }

            if (disabledIsInvisible)
                boundHolder.style.display = isDisabled ? DisplayStyle.None : DisplayStyle.Flex;
            else boundButton.SetEnabled(!isDisabled);
        }

        public ExplorerButtonDef(string label, Action<ExplorerEntryDef> onClick, string tooltip = null, EColor color = EColor.Clear)
            : base(label, color, onClick, tooltip) {}
    }
}