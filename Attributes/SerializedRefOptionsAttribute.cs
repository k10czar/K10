using UnityEngine;

namespace Rogue.RuntimeEditor
{
    public class SerializedRefOptionsAttribute : PropertyAttribute
    {
        public readonly bool forceCompact;

        public readonly bool canBeNull;
        public readonly string nullLabel;

        public SerializedRefOptionsAttribute(bool forceCompact)
        {
            this.forceCompact = forceCompact;
        }

        public SerializedRefOptionsAttribute(string nullLabel, bool forceCompact = false)
        {
            this.canBeNull = true;
            this.nullLabel = nullLabel;
            this.forceCompact = forceCompact;
        }
    }
}