using K10.EditorUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rogue.Explorer
{
    [UxmlElement]
    public partial class ExplorerEntryOptions : VisualElement
    {
        private Button compactButton;
        private VisualElement holder;

        private ExplorerEntryDef myData;

        private void SetRefs()
        {
            if (compactButton != null) return;

            compactButton = this.Q<Button>("compact-button");
            holder = this.Q<VisualElement>("holder");

            compactButton.clicked += OnCompactClicked;
            parent?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void SetOptions(ExplorerEntryDef dataRef)
        {
            SetRefs();

            myData = dataRef;

            if (myData.options == null || myData.options.Length == 0)
            {
                style.display = DisplayStyle.None;
                return;
            }

            style.display = DisplayStyle.Flex;
            holder.Clear();

            foreach (var entry in myData.options)
            {
                var button = new Button();
                holder.Add(button);
                button.clicked += () => entry.onClick(dataRef);
                button.focusable = false;

                entry.BindVisuals(button, button, true);
            }
        }

        private void OnCompactClicked()
        {
            var menu = new GenericMenu();

            foreach (var entry in myData.options)
            {
                if (!entry.isDisabled)
                    menu.AddItem(entry.label, () => entry.onClick(myData));
            }

            menu.ShowAsContext();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (Mathf.Approximately(evt.oldRect.width, evt.newRect.width)) return;
            ResetVisibility();
        }

        private void ResetVisibility()
        {
            // TODO
            var (dropVis, holderVis) = (DisplayStyle.None, DisplayStyle.Flex);

            compactButton.style.display = dropVis;
            holder.style.display = holderVis;
        }
    }
}