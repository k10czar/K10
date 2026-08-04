using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rogue.RuntimeEditor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EnumFilterAttribute : PropertyAttribute
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;

        public enum EFieldType
        {
            Field,
            Method,
            Property,
        }

        private readonly EFieldType fieldType;
        private readonly MethodInfo methodInfo;
        private readonly FieldInfo fieldInfo;
        private readonly PropertyInfo propertyInfo;

        private readonly object[] constList;

        public EnumFilterAttribute(params object[] constList) => this.constList = constList;

        public EnumFilterAttribute(Type providerType, string fieldName, EFieldType fieldType = EFieldType.Field)
        {
            this.fieldType = fieldType;

            if (fieldType is EFieldType.Field)
                fieldInfo = providerType.GetField(fieldName, Flags);
            else if (fieldType is EFieldType.Method)
                methodInfo = providerType.GetMethod(fieldName, Flags);
            else propertyInfo = providerType.GetProperty(fieldName, Flags);
        }

        public IEnumerable<object> GetValidList()
        {
            if (constList != null) return constList;

            var target = fieldType switch
            {
                EFieldType.Field => fieldInfo.GetValue(null),
                EFieldType.Method => methodInfo.Invoke(null, null),
                EFieldType.Property => propertyInfo.GetValue(null),
                _ => throw new ArgumentOutOfRangeException()
            };

            return (target as IEnumerable)?.Cast<object>();
        }
    }
}