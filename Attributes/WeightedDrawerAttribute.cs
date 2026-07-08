using UnityEngine;

namespace Rogue.RuntimeEditor
{
    public class WeightedDrawerAttribute : PropertyAttribute
    {
        public readonly string[] fieldNames;

        // the last field name MUST be a float value used as weight!
        public WeightedDrawerAttribute(params string[] fieldNames)
        {
            this.fieldNames = fieldNames;
        }

        public WeightedDrawerAttribute() : this("key", "value") {}
    }
}