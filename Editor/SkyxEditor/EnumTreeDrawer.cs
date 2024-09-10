#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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

            DrawEnumDropdown(position, property, EditorStyles.popup, fieldInfo.FieldType);

            EditorGUI.EndProperty();
        }

        public static void DrawEnumDropdown(Rect position, SerializedProperty property, Color color, GUIStyle style, Type fieldType)
        {
            SkyxLayout.SetBackgroundColor(color);
            position.y += 1;
            DrawEnumDropdown(position, property, style, fieldType);
            SkyxLayout.RestoreBackgroundColor();
        }

        public static void DrawEnumDropdown<T>(Rect position, T value, Action<object> callback) where T: Enum
            => DrawEnumDropdown(position, typeof(T), value, callback, Colors.Console.Primary, SkyxStyles.PopupStyle);

        public static void DrawEnumDropdown(Rect position, Type enumType, object enumObj, Action<object> callback, Color color, GUIStyle style)
        {
            SkyxLayout.SetBackgroundColor(color);
            position.y += 1;
            DrawEnumDropdown(position, enumType, enumObj, null, callback, style);
            SkyxLayout.RestoreBackgroundColor();
        }

        private static void DrawEnumDropdown(Rect position, SerializedProperty property, GUIStyle style, Type fieldType)
        {
            var enumType = fieldType;
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                enumType = fieldType.GenericTypeArguments[0];

            if (!enumType.IsEnum)
            {
                EditorGUI.PropertyField(position, property, GUIContent.none);
                return;
            }

            var enumObj = Enum.ToObject(enumType, property.intValue);

            DrawEnumDropdown(position, enumType, enumObj, property, null, style);
        }

        private static void DrawEnumDropdown(Rect position, Type enumType, object enumObj, SerializedProperty property, Action<object> callback, GUIStyle style)
        {
            var name = new GUIContent(ObjectNames.NicifyVariableName(enumObj.ToString()));

            if (!EditorGUI.DropdownButton(position, name, FocusType.Passive, style)) return;
            var state = new AdvancedDropdownState();

            var enumTreeType = typeof(EnumTreeNode<>).MakeGenericType(enumType);
            var instanceProperty = enumTreeType.GetProperty("Instance");
            var tree = instanceProperty!.GetMethod.Invoke(null, null);

            var genericDropdownType = typeof(TreeAdvancedDropdown<>);
            var specificDropdownType = genericDropdownType.MakeGenericType(enumType);
            var dropdown = (AdvancedDropdown) Activator.CreateInstance(specificDropdownType, state, tree, property, callback);

            var dropdownRect = new Rect(position);
            dropdown.Show(dropdownRect);
        }
    }

    internal class TreeAdvancedDropdown<T> : AdvancedDropdown where T : Enum
    {
        private readonly TreeNode<T> treeNode;
        private readonly SerializedProperty property;
        private readonly Action<object> callback;

        private readonly bool canCreateNodes;
        private readonly string enumDeclarationFilePath;

        public TreeAdvancedDropdown(AdvancedDropdownState state, EnumTreeNode<T> treeNode, SerializedProperty property, Action<object> callback) : base(state)
        {
            this.treeNode = treeNode;
            this.property = property;
            this.callback = callback;

            var definitionAttributes = typeof(T).GetCustomAttributes(typeof(ExpandableEnumTreeAttribute), true);
            canCreateNodes = definitionAttributes.Length > 0;
            if (canCreateNodes) enumDeclarationFilePath = ((ExpandableEnumTreeAttribute)definitionAttributes[0]).path;

            minimumSize = new Vector2(300, 300);
        }

        protected override AdvancedDropdownItem BuildRoot() => BuildNodeDropdown(treeNode);

        private AdvancedDropdownItem BuildNodeDropdown(TreeNode<T> currentTreeNode)
        {
            var dropdown = new AdvancedDropdownItem(currentTreeNode.TreeDisplayName);
            var children = currentTreeNode.GetChildren();

            var hasValidChildren = false;
            var hasNodesWithChildren = false;

            foreach (var node in children)
            {
                if (node.IsValid)
                {
                    var isSelected = property?.enumValueFlag == (int)(object)node.Value;
                    dropdown.AddChild(new TreeAdvancedDropdownItem<T>(node, isSelected));
                    hasValidChildren = true;
                }

                if (node.HasChildren) hasNodesWithChildren = true;
            }

            if (hasValidChildren && hasNodesWithChildren) dropdown.AddSeparator();

            foreach (var node in children)
            {
                if (node.HasChildren) dropdown.AddChild(BuildNodeDropdown(node));
            }

            if (canCreateNodes)
            {
                dropdown.AddSeparator();
                dropdown.AddChild(new NewNodeAdvancedDropdownItem<T>(currentTreeNode));
            }

            return dropdown;
        }

        private void TreeNodeSelected(TreeAdvancedDropdownItem<T> treeItem)
        {
            Debug.Assert(treeItem.isValid, $"Selected invalid entry! {treeItem.value}");

            callback?.Invoke(treeItem.value);

            if (property == null) return;

            property.serializedObject.Update();
            property.intValue = (int)(object) treeItem.value;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void NewNodeSelected(NewNodeAdvancedDropdownItem<T> newNodeItem)
        {
            var parent = newNodeItem.parent;
            NewEnumNodeWindow.OpenWindow(parent.Value, parent.Path, enumDeclarationFilePath);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TreeAdvancedDropdownItem<T> treeItem) TreeNodeSelected(treeItem);
            else if (item is NewNodeAdvancedDropdownItem<T> newNodeItem) NewNodeSelected(newNodeItem);
        }
    }

    internal class TreeAdvancedDropdownItem<T> : AdvancedDropdownItem where T : Enum
    {
        public readonly T value;
        public readonly bool isValid;

        public TreeAdvancedDropdownItem(TreeNode<T> node, bool isSelected)
            : base($"{(isSelected ? "✓ " : "")}{node.ValueDisplayName}")
        {
            value = node.Value;
            isValid = node.IsValid;
        }
    }

    internal class NewNodeAdvancedDropdownItem<T> : AdvancedDropdownItem where T : Enum
    {
        public readonly TreeNode<T> parent;

        public NewNodeAdvancedDropdownItem(TreeNode<T> node) : base("+ Create Node")
        {
            parent = node;
        }
    }
}
#endif