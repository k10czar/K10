using System;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public class SerializeFieldContractResolver : DefaultContractResolver
    {
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
            if (member is PropertyInfo propertyInfo && propertyInfo.CanRead && propertyInfo.CanWrite)
            {
                // Get the backing field for the property
                FieldInfo backingField = propertyInfo.DeclaringType.GetField(
                    $"<{propertyInfo.Name}>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                // If the backing field exists and has the [SerializeField] attribute, include the property
                if (backingField != null && backingField.GetCustomAttribute<SerializeField>() != null)
                {
                    property.Ignored = false; // Ensure the property is not ignored
                    property.Readable = true; // Make the property readable
                    property.Writable = true; // Make the property writable
                }
            }

            return property;
        }

        protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            // Include private fields in the list of properties
            var properties = base.CreateProperties(type, memberSerialization);

            // Add private fields to the properties list
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // Skip fields that are already included (e.g., public fields)
                if (properties.Any(p => p.UnderlyingName == field.Name))
                    continue;

                // Create a JsonProperty for the private field
                var property = CreateProperty(field, memberSerialization);
                properties.Add(property);
            }

            return properties;
        }
    }

    public class UnityObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Object).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonObject = new JObject { { "instanceID", ((Object) value)!.GetInstanceID() } };
            jsonObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var instanceID = jsonObject["instanceID"]!.ToObject<int>();

            var unityObject = EditorUtility.InstanceIDToObject(instanceID);
            if (unityObject != null) return unityObject;

            if (instanceID != 0) Debug.LogError($"Unity object with instanceID {instanceID} not found.");

            return null;
        }
    }
}