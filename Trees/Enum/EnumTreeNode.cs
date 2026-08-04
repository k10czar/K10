using System;
using System.Collections.Generic;

namespace Skyx.Trees
{
    public class EnumTreeNode<T> : TreeNode<T> where T: Enum
    {
        public static EnumTreeNode<T> Instance { get; } = new();
        private static bool useEnumAsKey;

        public static IEnumerable<T> GetChildrenValues(T value) => Instance.GetChildrenValues(Instance.GetNodeInfo(value).path);
        private static IEnumerable<T> GetEnumValues() => (T[]) Enum.GetValues(typeof(T));

        protected override TreeNodeInfo<T> GetNodeInfo(T value)
        {
#if UNITY_EDITOR
            var prettyName = UnityEditor.ObjectNames.NicifyVariableName(value.ToString());
#else
            var prettyName = value.ToString();
#endif
            var nodeInfo = new TreeNodeInfo<T>()
            {
                value = value,
                path = new Queue<object>(),

                order = int.MinValue,
                treeName = prettyName,
                valueName = prettyName,
                hide = false,
            };

            var attribute = GetEnumTreeAttribute(value);
            if (attribute != null)
            {
                nodeInfo.path = new Queue<object>(attribute.path);
                nodeInfo.order = attribute.order == int.MinValue ? Convert.ToInt32(value) : attribute.order;
                nodeInfo.hide = attribute.hide;

                if (!string.IsNullOrEmpty(attribute.valueDisplayName)) nodeInfo.valueName = attribute.valueDisplayName;
                if (!string.IsNullOrEmpty(attribute.treeDisplayName)) nodeInfo.treeName = attribute.treeDisplayName;
            }

            nodeInfo.path.Enqueue(useEnumAsKey ? value : value.ToString());

            return nodeInfo;
        }

        protected override TreeNode<T> InstantiateNode(object key, string path) => new EnumTreeNode<T>(key, path);

        private EnumTreeNode(object key, string path) : base(key, path) {}

        private EnumTreeNode()
        {
            var enumValues = GetEnumValues();

            foreach (var nodeValue in enumValues)
            {
                var attribute = GetEnumTreeAttribute(nodeValue);
                if (attribute == null || attribute.path == null || attribute.path.Length == 0) continue;

                useEnumAsKey = attribute.path[0] is not string;
                break;
            }

            foreach (var nodeValue in enumValues)
                CreateNode(nodeValue);
        }

        private static EnumTreeAttribute GetEnumTreeAttribute(T value)
        {
            var enumType = value.GetType();
            var enumName = Enum.GetName(enumType, value);
            var fieldInfo = enumType.GetField(enumName);

            var attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(EnumTreeAttribute));
            return attribute == null ? new EnumTreeAttribute() : attribute as EnumTreeAttribute;
        }
    }
}