using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class StringPicker
    {
        public static void Draw(Rect position, string[] validValues, SerializedProperty property)
        {
            Draw(position, validValues, property.stringValue, OnChanged);

            void OnChanged(string newValue)
            {
                property.stringValue = newValue;
                property.Apply();
            }
        }

        public static void Draw(Rect position, string[] validValues, string currentSelection, Action<string> callback)
        {
            var state = new AdvancedDropdownState();
            var dropdown = new StringAdvancedDropdown(state, validValues, currentSelection, callback);
            dropdown.Show(position);
        }
    }

    internal class StringAdvancedDropdown : AdvancedDropdown
    {
        private readonly Action<string> callback;
        private readonly string currentSelection;
        private readonly string[] validValues;

        public StringAdvancedDropdown(AdvancedDropdownState state, string[] validValues, string currentSelection, Action<string> callback) : base(state)
        {
            this.callback = callback;
            this.currentSelection = currentSelection;
            this.validValues = validValues;

            minimumSize = new Vector2(300, 300);
        }

        protected override AdvancedDropdownItem BuildRoot() => BuildNodeDropdown();

        private AdvancedDropdownItem BuildNodeDropdown()
        {
            var root = new AdvancedDropdownItem("Valid Values");

            foreach (var possibleValue in validValues)
            {
                var isSelected = currentSelection == possibleValue;
                root.AddChild(new StringAdvancedDropdownItem(possibleValue, isSelected));
            }

            return root;
        }

        private void TreeNodeSelected(StringAdvancedDropdownItem treeItem) => callback.Invoke(treeItem.value);

        protected override void ItemSelected(AdvancedDropdownItem item) => TreeNodeSelected(item as StringAdvancedDropdownItem);
    }

    internal class StringAdvancedDropdownItem : AdvancedDropdownItem
    {
        public readonly string value;

        public StringAdvancedDropdownItem(string value, bool isSelected) : base($"{(isSelected ? "✓ " : "")}{value}")
        {
            this.value = value;
        }
    }
}