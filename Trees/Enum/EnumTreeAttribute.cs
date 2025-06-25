using System;
using System.Linq;

namespace Skyx.Trees
{
    public class EnumTreeAttribute : Attribute
    {
        public readonly object[] path;
        public readonly int order;
        public readonly string valueDisplayName;
        public readonly string treeDisplayName;
        public readonly bool hide;

        public EnumTreeAttribute(object[] path = null, string valueDisplayName = "", string treeDisplayName = "", int order = int.MinValue, bool hide = false)
        {
            this.path = path ?? Array.Empty<object>();
            this.valueDisplayName = valueDisplayName;
            this.treeDisplayName = treeDisplayName;
            this.order = order;
            this.hide = hide;
        }

        public EnumTreeAttribute(bool hide) : this(null, hide: hide) {}
        public EnumTreeAttribute(params object[] path) : this(path, "") {}
        public EnumTreeAttribute(int _, string valueDisplayName) : this(null, valueDisplayName, valueDisplayName) {}

        public EnumTreeAttribute(string[] path, string valueDisplayName = "") : this(path.ToArray<object>(), valueDisplayName) {}
    }
}