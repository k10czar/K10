using System;
using UnityEngine;

namespace Rogue.RuntimeEditor
{
    public class ClassTreeTypePickerAttribute : PropertyAttribute
    {
        public readonly Type targetType;

        public ClassTreeTypePickerAttribute(Type targetType)
        {
            this.targetType = targetType;
        }
    }
}