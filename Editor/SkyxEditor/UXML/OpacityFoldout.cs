using UnityEngine.UIElements;

namespace Rogue.REditor
{
    [UxmlElement]
    public partial class OpacityFoldout : VisualElement
    {
        public readonly Toggle Toggle;
        public readonly VisualElement Content;

        public OpacityFoldout() : this("") {}

        public OpacityFoldout(string label)
        {
            Toggle = new Toggle() { focusable = false };
            Content = new VisualElement() { name = "unity-content" };

            Add(Toggle);
            Add(Content);

            AddToClassList(Foldout.ussClassName); // ".unity-foldout"
            Toggle.AddToClassList(Foldout.toggleUssClassName); // ".unity-foldout__toggle"
            Content.AddToClassList(Foldout.contentUssClassName); // ".unity-foldout__content"

            SetLabel(label);

            Toggle.RegisterValueChangedCallback(OnToggleChange);
            ForceState(true);
        }

        private void OnToggleChange(ChangeEvent<bool> evt) => Content.SetVisibleOpacity(evt.newValue);

        public void SetLabel(string newLabel) => Toggle.text = newLabel;

        public void ForceState(bool isToggled) => Toggle.value = isToggled;

        public void HideToggle(bool shouldHide)
        {
            Toggle.Q("unity-checkmark").SetVisible(!shouldHide);
        }
    }
}