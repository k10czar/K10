using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public static class JsonSerializationLib
    {
        #region Serialized Properties

        public static Action<SerializedProperty> onPasted;

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

            var currentValue = property.GetValue();
            var prePasteData = currentValue is IPasteSerializationFix prePasteFix ? prePasteFix.GetPrePasteData() : null;

            var deserializedObject = JsonConvert.DeserializeObject(json, valueType, GetSerializationSettings());

            if (deserializedObject is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(prePasteData);

            property.SetValue(deserializedObject);
            property.ApplyDirectChanges();

            onPasted?.Invoke(property);
        }

        #endregion

        #region Direct Object Manipulation

        public static void CopyValues<T>(T source, T target) where T : class
        {
            var json = GetJson(source);
            SetValueFromJson(target, json);
        }

        public static string GetJson(object target) => JsonConvert.SerializeObject(target, GetSerializationSettings());

        public static void SetValueFromJson(object target, string json)
        {
            var prePasteData = target is IPasteSerializationFix prePasteFix ? prePasteFix.GetPrePasteData() : null;

            JsonConvert.PopulateObject(json, target, GetSerializationSettings());

            if (target is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(prePasteData);
        }

        public static object GetFromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject(json, GetSerializationSettings());

            if (obj is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(null);

            return obj;
        }

        public static T GetFromJson<T>(string json) where T : class
        {
            var obj = JsonConvert.DeserializeObject<T>(json, GetSerializationSettings());

            if (obj is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(null);

            return obj;
        }

        public static List<object> GetObjectListFromJson(string json)
        {
            var settings = GetSerializationSettings();

            var jArray = JsonConvert.DeserializeObject(json, settings) as JArray;
            if (jArray == null) return null;

            var serializer = JsonSerializer.Create(settings);
            var entries = jArray.Select(token => token.ToObject<object>(serializer)).ToList();

            return entries;
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

    public interface IPasteSerializationFix
    {
        public object GetPrePasteData() => null;
        public void FixSerializationPostPaste(object prePasteData);
    }
}