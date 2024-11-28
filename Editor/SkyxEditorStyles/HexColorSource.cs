using System;

namespace Skyx.CustomEditor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HexColorSource : Attribute
    {
        public readonly string hexColor;

        public HexColorSource(string hexColor) => this.hexColor = hexColor;
    }
}