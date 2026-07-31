using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Rogue.REditor
{
    public class SerializeFieldContractResolver : DefaultContractResolver
    {
        private static FieldInfo GetBackingField(PropertyInfo propertyInfo)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var currentType = propertyInfo.DeclaringType;

            while (currentType != null)
            {
                var backingField = currentType.GetField($"<{propertyInfo.Name}>k__BackingField", flags);
                if (backingField != null) return backingField;

                currentType = currentType.BaseType;
            }

            return null;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // Check if the member is a field and has the [SerializeField] attribute
            if (member is FieldInfo field && field.GetCustomAttribute<SerializeField>() != null)
            {
                property.Ignored = false; // Ensure the field is not ignored
                property.Readable = true; // Make the field readable
                property.Writable = true; // Make the field writable
            }

            // Check if the member is a property and its backing field has the [SerializeField] attribute
            if (member is PropertyInfo propertyInfo)
            {
                var backingField = GetBackingField(propertyInfo);

                // If the backing field exists and has the [SerializeField] attribute, include the property
                if (backingField != null && backingField.GetCustomAttribute<SerializeField>() != null)
                {
                    property.Ignored = false;
                    property.Readable = true;
                    property.Writable = true;
                }
                else
                {
                    property.Ignored = true;
                    property.Readable = false;
                    property.Writable = false;
                }
            }

            if (member.GetCustomAttribute<SerializeReference>() != null)
                property.ObjectCreationHandling = ObjectCreationHandling.Replace;

            return property;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            var properties = base.CreateProperties(type, memberSerialization);

            for (var currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                var fields = currentType.GetFields(flags);

                foreach (var field in fields)
                {
                    // Skip fields that are already included (e.g., public fields)
                    if (properties.Any(p => p.UnderlyingName == field.Name))
                        continue;

                    // Create a JsonProperty for the private field
                    var property = CreateProperty(field, memberSerialization);
                    properties.Add(property);
                }
            }

            return properties;
        }
    }
}