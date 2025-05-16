using System;

namespace Skyx.Trees
{
    public class ClassTreeAttribute : Attribute
    {
        public readonly string[] path;
        public readonly string displayName;

        public ClassTreeAttribute(params string[] pathIncludingDisplayName)
        {
            path = pathIncludingDisplayName[..^1];
            displayName = pathIncludingDisplayName[^1];
        }

        public ClassTreeAttribute(bool implyName, params string[] path)
        {
            this.path = path;
            displayName = null;
        }
    }
}