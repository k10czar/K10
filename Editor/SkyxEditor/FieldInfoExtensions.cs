using System.Reflection;
using UnityEngine;

namespace Rogue.REditor
{
    public static class FieldInfoExtensions
    {
        public static bool TryGetAttribute<T>(this FieldInfo fieldInfo, out T attribute) where T : PropertyAttribute
        {
            var customAttributes = fieldInfo.GetCustomAttributes(typeof(T), true);

            if (customAttributes.Length != 1)
            {
                attribute = null;
                return false;
            }

            attribute = (T)customAttributes[0];
            return true;
        }
    }
}