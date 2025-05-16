using System;
using System.Collections.Generic;
using System.Reflection;

namespace Skyx.Trees
{
    public class ClassTreeNode : TreeNode<Type>
    {
        #region Static

        private static readonly Dictionary<Type, ClassTreeNode> cache = new();

        public static ClassTreeNode Get(Type target)
        {
            if (cache.TryGetValue(target, out var result)) return result;

            var newEntry = new ClassTreeNode(target);
            cache.Add(target, newEntry);

            return newEntry;
        }

        #endregion

        public readonly Type parentType;

        protected override TreeNodeInfo<Type> GetNodeInfo(Type value)
        {
#if UNITY_EDITOR
            var prettyName = UnityEditor.ObjectNames.NicifyVariableName(value.Name);
#else
            var prettyName = value.Name;
#endif

            var nodeInfo = new TreeNodeInfo<Type>()
            {
                value = value,
                path = new Queue<object>(),

                order = int.MinValue,
                treeName = prettyName,
                valueName = prettyName,
                hide = false,
            };

            var attribute = GetClassTreeAttribute(value);
            if (attribute != null)
            {
                nodeInfo.path = new Queue<object>(attribute.path);
                if (!string.IsNullOrEmpty(attribute.displayName)) nodeInfo.valueName = attribute.displayName;
            }

            nodeInfo.path.Enqueue(value.Name);

            return nodeInfo;
        }

        protected override TreeNode<Type> InstantiateNode(object key, string path) => new ClassTreeNode(key, path);
        private ClassTreeNode(object key, string path) : base(key, path) {}

        private ClassTreeNode(Type parentType)
        {
            this.parentType = parentType;
            var allTypes = TypeListDataCache.GetFrom(parentType).GetTypes();

            foreach (var nodeValue in allTypes) CreateNode(nodeValue);
        }

        private static ClassTreeAttribute GetClassTreeAttribute(Type target)
        {
            var attribute = target.GetCustomAttribute(typeof(ClassTreeAttribute), false);
            return attribute as ClassTreeAttribute;
        }
    }
}