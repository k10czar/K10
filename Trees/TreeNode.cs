using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Skyx.Trees
{
    public abstract class TreeNode<T> : IComparable
    {
        public T Value { get; protected set; }
        public bool IsValid { get; protected set; }
        private int Order { get; set; }
        public string ValueDisplayName { get; protected set; }
        public string TreeDisplayName { get; protected set; }
        private object Key { get; }
        public string Path { get; private set; }


        private readonly bool isRoot;

        private readonly Dictionary<object, TreeNode<T>> children = new();
        public bool HasChildren => children.Count > 0;

        public List<TreeNode<T>> GetChildren()
        {
            var values = children.Values.ToList();
            values.Sort();

            return values;
        }

        private string SerializePath(IEnumerable<object> path) => string.Join("/", path);

        protected List<T> GetNodesInPath(Queue<object> keyOrder, bool filterValid = true)
        {
            Debug.Assert(isRoot, "This method only works when called from root nodes");

            var nodesInPath = new List<T>();
            var currentNode = this;

            while (keyOrder.Count > 0)
            {
                var key = keyOrder.Dequeue();
                if (!currentNode.HasChildNode(key)) return nodesInPath;

                currentNode = currentNode.GetChildNode(key);
                if (!filterValid || currentNode.IsValid) nodesInPath.Add(currentNode.Value);
            }

            return nodesInPath;
        }

        protected abstract TreeNodeInfo<T> GetNodeInfo(T value);

        protected abstract TreeNode<T> InstantiateNode(object key, string path);

        private TreeNode<T> GetChildNode(object key) => children[key];

        private TreeNode<T> AddChildNode(object key, string path)
        {
            children[key] = InstantiateNode(key, path);
            return children[key];
        }

        protected TreeNode<T> CreateTreePath(Queue<object> path)
        {
            var currentNode = this;
            var pathTraveled = new List<object>();

            while (path.Count > 0)
            {
                var nextKey = path.Dequeue();

                currentNode = currentNode.HasChildNode(nextKey)
                    ? currentNode.GetChildNode(nextKey)
                    : currentNode.AddChildNode(nextKey, SerializePath(pathTraveled));

                pathTraveled.Add(nextKey);
            }

            return currentNode;
        }

        private void SetNodeValues(TreeNodeInfo<T> nodeInfo, string path)
        {
            Value = nodeInfo.value;
            Order = nodeInfo.order;
            ValueDisplayName = nodeInfo.valueName;
            TreeDisplayName = nodeInfo.treeName;
            Path = path;

            IsValid = !nodeInfo.hide;
        }

        protected void CreateNode(T newValue)
        {
            var nodeInfo = GetNodeInfo(newValue);
            var path = SerializePath(nodeInfo.path);

            var leaf = CreateTreePath(nodeInfo.path);
            leaf.SetNodeValues(nodeInfo, path);
        }

        private bool HasChildNode(object key) => children.ContainsKey(key);

        protected TreeNode(object key, string path)
        {
            Key = key;
            ValueDisplayName = TreeDisplayName = key.ToString();
            Path = path;
        }

        protected TreeNode()
        {
            isRoot = true;
            ValueDisplayName = TreeDisplayName = "Root";
        }

        public override string ToString()
        {
            var name = IsValid ? ValueDisplayName : TreeDisplayName;
            return $"Node {Key}/{Value} ({name}) | [{children.Count}] > {string.Join(", ", children.Select(entry => entry.Key))}";
        }

        public int CompareTo(object obj)
        {
            var otherNode = (TreeNode<T>) obj;

            if (otherNode == null) return 1;

            if (!IsValid && !otherNode.IsValid)
                return string.Compare(Key.ToString(), otherNode.Key.ToString(), StringComparison.Ordinal);

            if (!IsValid) return -1;
            if (!otherNode.IsValid) return 1;

            return Order.CompareTo(otherNode.Order);
        }

        // TODO: Go over all nodes and order them
    }
}