using System;
using System.Collections.Generic;
using Skyx.Trees;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Rogue.REditor
{
    public static class ClassTreePicker
    {
        public static void Draw(Rect position, Type parentType, Type currentSelection, Action<Type> callback, IEnumerable<Type> validTypes = null, string nullLabel = null)
        {
            var state = new AdvancedDropdownState();

            var tree = ClassTreeNode.Get(parentType, validTypes);
            var dropdown = new ClassTreeAdvancedDropdown(state, tree, currentSelection, callback, nullLabel);
            dropdown.Show(position);
        }
    }

    internal class ClassTreeAdvancedDropdown : AdvancedDropdown
    {
        private readonly ClassTreeNode treeNode;
        private readonly Type currentSelection;
        private readonly Action<Type> callback;
        private readonly string nullLabel;

        public ClassTreeAdvancedDropdown(AdvancedDropdownState state, ClassTreeNode treeNode, Type currentSelection, Action<Type> callback, string nullLabel) : base(state)
        {
            this.treeNode = treeNode;
            this.callback = callback;
            this.currentSelection = currentSelection;
            this.nullLabel = nullLabel;

            minimumSize = new Vector2(300, 300);
        }

        protected override AdvancedDropdownItem BuildRoot() => BuildNodeDropdown(treeNode, true);

        private AdvancedDropdownItem BuildNodeDropdown(TreeNode<Type> currentTreeNode, bool isRoot)
        {
            var dropdown = new AdvancedDropdownItem(currentTreeNode.TreeDisplayName);
            var children = currentTreeNode.GetChildren();

            var hasValidChildren = false;
            var hasNodesWithChildren = false;

            foreach (var node in children)
            {
                if (node.IsValid)
                {
                    var isSelected = currentSelection == node.Value;
                    dropdown.AddChild(new ClassTreeAdvancedDropdownItem(node, isSelected));
                    hasValidChildren = true;
                }

                if (node.HasChildren) hasNodesWithChildren = true;
            }

            if (isRoot && !string.IsNullOrEmpty(nullLabel))
                dropdown.AddChild(new ClassTreeAdvancedDropdownItem(nullLabel, currentSelection == null));

            if (hasValidChildren && hasNodesWithChildren) dropdown.AddSeparator();

            foreach (var node in children)
            {
                if (node.HasChildren) dropdown.AddChild(BuildNodeDropdown(node, false));
            }

            return dropdown;
        }

        private void TreeNodeSelected(ClassTreeAdvancedDropdownItem treeItem)
        {
            Debug.Assert(treeItem.isValid, $"Selected invalid entry! {treeItem.value}");
            callback.Invoke(treeItem.value);
        }

        protected override void ItemSelected(AdvancedDropdownItem item) => TreeNodeSelected(item as ClassTreeAdvancedDropdownItem);
    }

    internal class ClassTreeAdvancedDropdownItem : AdvancedDropdownItem
    {
        public readonly Type value;
        public readonly bool isValid;

        public ClassTreeAdvancedDropdownItem(TreeNode<Type> node, bool isSelected) : base($"{(isSelected ? "✓ " : "")}{node.ValueDisplayName}")
        {
            value = node.Value;
            isValid = node.IsValid;
        }

        public ClassTreeAdvancedDropdownItem(string nullLabel, bool isSelected)
            : base($"{(isSelected ? "✓ " : "")}{nullLabel}")
        {
            value = null;
            isValid = true;
        }
    }
}