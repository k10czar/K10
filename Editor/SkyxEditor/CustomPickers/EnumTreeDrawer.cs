#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Skyx.Trees;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(Enum), true)]
    public sealed class EnumTreeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var fieldType = fieldInfo.FieldType.IsArray
                ? property.propertyType is SerializedPropertyType.Enum
                    ? fieldInfo.FieldType.GetElementType()
                    : throw new Exception($"Property is not an Enum! {property} | {fieldInfo.FieldType}")
                : fieldInfo.FieldType;

            DrawEnumDropdown(position, property, fieldType, null, false);

            EditorGUI.EndProperty();
        }

        public static void DrawEnumDropdown(Rect position, SerializedProperty property, EColor color, Type fieldType, IEnumerable<object> validList = null, bool isIncludeList = false)
        {
            using var backgroundScope = BackgroundColorScope.Set(color);
            DrawEnumDropdown(position, property, fieldType, validList, isIncludeList);
        }

        public static void DrawEnumDropdown<T>(Rect position, T value, Action<object> callback, EColor color = EColor.Primary, IEnumerable<object> validList = null, bool isIncludeList = false) where T: Enum
        {
            using var backgroundScope = BackgroundColorScope.Set(color);
            position.y += 1;
            DrawEnumDropdown(position, typeof(T), value, null, callback, validList, isIncludeList);
        }

        private static void DrawEnumDropdown(Rect position, SerializedProperty property, Type fieldType, IEnumerable<object> validList, bool isIncludeList)
        {
            var enumType = fieldType;
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                enumType = fieldType.GenericTypeArguments[0];

            if (!enumType.IsEnum)
            {
                Debug.LogError("Calling Draw enum on a non enum type!");
                EditorGUI.PropertyField(position, property, GUIContent.none);
                return;
            }

            var enumObj = Enum.ToObject(enumType, property.intValue);

            DrawEnumDropdown(position, enumType, enumObj, property, null, validList, isIncludeList);
        }

        private static void DrawEnumDropdown(Rect position, Type enumType, object enumObj, SerializedProperty property, Action<object> callback, IEnumerable<object> validList, bool isIncludeList)
        {
            var name = new GUIContent(ObjectNames.NicifyVariableName(enumObj.ToString()));

            if (!EditorGUI.DropdownButton(position, name, FocusType.Passive, EditorStyles.popup)) return;
            var state = new AdvancedDropdownState();

            var enumTreeType = typeof(EnumTreeNode<>).MakeGenericType(enumType);
            var instanceProperty = enumTreeType.GetProperty("Instance");
            var tree = instanceProperty!.GetMethod.Invoke(null, null);

            var genericDropdownType = typeof(EnumTreeAdvancedDropdown<>);
            var specificDropdownType = genericDropdownType.MakeGenericType(enumType);
            var dropdown = (AdvancedDropdown) Activator.CreateInstance(specificDropdownType, state, tree, property, callback, validList, isIncludeList);

            var dropdownRect = new Rect(position);
            dropdown.Show(dropdownRect);
        }
    }

    internal class EnumTreeAdvancedDropdown<T> : AdvancedDropdown where T : Enum
    {
        private readonly TreeNode<T> treeNode;
        private readonly SerializedProperty property;
        private readonly Action<object> callback;

        private readonly bool canCreateNodes;
        private readonly string enumDeclarationFilePath;

        private readonly IEnumerable<object> validList;
        private readonly bool listIsInclude; // or exclude

        public EnumTreeAdvancedDropdown(AdvancedDropdownState state, EnumTreeNode<T> treeNode, SerializedProperty property, Action<object> callback, IEnumerable<object> validList, bool isIncludeList) : base(state)
        {
            this.treeNode = treeNode;
            this.property = property;
            this.callback = callback;
            this.validList = validList;
            this.listIsInclude = isIncludeList;

            var definitionAttributes = typeof(T).GetCustomAttributes(typeof(ExpandableEnumTreeAttribute), true);
            canCreateNodes = definitionAttributes.Length > 0;
            if (canCreateNodes) enumDeclarationFilePath = ((ExpandableEnumTreeAttribute)definitionAttributes[0]).path;

            minimumSize = new Vector2(300, 300);
        }

        protected override AdvancedDropdownItem BuildRoot() => BuildNodeDropdown(treeNode);

        private bool IsNodeIncluded(TreeNode<T> node)
        {
            if (validList == null) return true;

            var contains = validList.Contains(node.Value);
            if (listIsInclude && contains) return true;

            if (node.HasChildren)
            {
                var children = node.GetChildren();

                foreach (var child in children)
                {
                    if (IsNodeIncluded(child)) return true;
                }
            }

            return listIsInclude == contains;
        }

        private AdvancedDropdownItem BuildNodeDropdown(TreeNode<T> currentTreeNode)
        {
            var dropdown = new AdvancedDropdownItem(currentTreeNode.TreeDisplayName);
            var children = currentTreeNode.GetChildren();

            var hasValidChildren = false;
            var hasNodesWithChildren = false;

            foreach (var node in children)
            {
                if (!IsNodeIncluded(node)) continue;

                if (node.IsValid)
                {
                    var isSelected = property?.intValue == node.GetIntValue();
                    dropdown.AddChild(new EnumTreeAdvancedDropdownItem<T>(node, isSelected));
                    hasValidChildren = true;
                }

                if (node.HasChildren) hasNodesWithChildren = true;
            }

            if (hasValidChildren && hasNodesWithChildren) dropdown.AddSeparator();

            foreach (var node in children)
            {
                if (node.HasChildren && IsNodeIncluded(node))
                    dropdown.AddChild(BuildNodeDropdown(node));
            }

            if (canCreateNodes)
            {
                dropdown.AddSeparator();
                dropdown.AddChild(new NewEnumNodeAdvancedDropdownItem<T>(currentTreeNode));
            }

            return dropdown;
        }

        private void TreeNodeSelected(EnumTreeAdvancedDropdownItem<T> treeItem)
        {
            Debug.Assert(treeItem.IsValid, $"Selected invalid entry! {treeItem.Value}");

            callback?.Invoke(treeItem.Value);

            if (property == null) return;

            property.intValue = treeItem.GetIntValue();
            property.Apply();
        }

        private void NewNodeSelected(NewEnumNodeAdvancedDropdownItem<T> newNodeItem)
        {
            var parent = newNodeItem.parent;
            NewEnumNodeWindow.OpenWindow(parent.Value, parent.Path, enumDeclarationFilePath);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is EnumTreeAdvancedDropdownItem<T> treeItem) TreeNodeSelected(treeItem);
            else if (item is NewEnumNodeAdvancedDropdownItem<T> newNodeItem) NewNodeSelected(newNodeItem);
        }
    }

    internal class EnumTreeAdvancedDropdownItem<T> : AdvancedDropdownItem where T : Enum
    {
        private readonly TreeNode<T> node;

        public T Value => node.Value;
        public bool IsValid => node.IsValid;
        public int GetIntValue() => node.GetIntValue();

        public EnumTreeAdvancedDropdownItem(TreeNode<T> node, bool isSelected)
            : base($"{(isSelected ? "✓ " : "")}{node.ValueDisplayName}")
        {
            this.node = node;
        }
    }

    internal class NewEnumNodeAdvancedDropdownItem<T> : AdvancedDropdownItem where T : Enum
    {
        public readonly TreeNode<T> parent;

        public NewEnumNodeAdvancedDropdownItem(TreeNode<T> node) : base("+ Create Node")
        {
            parent = node;
        }
    }
}
#endif