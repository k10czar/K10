using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public static class JsonSerializationLib
    {
        #region Serialized Properties

        public static void CopyValue<T>(this SerializedProperty property, T value, string reason) where T : class
        {
            var json = JsonConvert.SerializeObject(value, GetSerializationSettings());
            SetValueFromJson(property, json, typeof(T), reason);
        }

        public static string GetJson(this SerializedProperty property)
        {
            var value = property.GetValue();
            return JsonConvert.SerializeObject(value, GetSerializationSettings());
        }

        public static void SetValueFromJson(this SerializedProperty property, string json, Type valueType, string reason)
        {
            property.PrepareForChanges(reason);

            var deserializedObject = JsonConvert.DeserializeObject(json, valueType, GetSerializationSettings());
            property.SetValue(deserializedObject);

            property.ApplyDirectChanges();
        }

        #endregion

        #region Direct Object Manipulation

        public static void CopyValues<T>(T source, T target) where T : class
        {
            var json = GetJson(source);
            Debug.Log(json);
            SetValueFromJson(target, json);
        }

        public static string GetJson(object target) => JsonConvert.SerializeObject(target, GetSerializationSettings());

        public static void SetValueFromJson(object target, string json)
        {
            JsonConvert.PopulateObject(json, target, GetSerializationSettings());
        }

        #endregion

        private static JsonSerializerSettings GetSerializationSettings() => new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,

            ContractResolver = new SerializeFieldContractResolver(),
            Converters = { new UnityObjectConverter() },
        };
    }
}