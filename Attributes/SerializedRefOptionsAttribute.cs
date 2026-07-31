using UnityEngine;

namespace Rogue.RuntimeEditor
{
    public class SerializedRefOptionsAttribute : PropertyAttribute
    {
        public bool forceCompact;
        public bool canBeNull;

        public SerializedRefOptionsAttribute(bool forceCompact = false, bool canBeNull = false)
        {
            this.forceCompact = forceCompact;
            this.canBeNull = canBeNull;
        }
    }
}