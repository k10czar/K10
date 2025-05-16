using System;
using Application = UnityEngine.Device.Application;

namespace Skyx.Trees
{
    public class ExpandableEnumTreeAttribute : Attribute
    {
        public readonly string path;

        public ExpandableEnumTreeAttribute(string relativePath)
        {
            path = $"{Application.dataPath}/{relativePath}";
        }
    }
}