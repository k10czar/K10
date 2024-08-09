using System.Collections.Generic;

namespace Skyx.Trees
{
    public class TreeNodeInfo<T>
    {
        public Queue<object> path;
        public T value;

        public int order;
        public string valueName;
        public string treeName;
        public bool hide;
    }
}