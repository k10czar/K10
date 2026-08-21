using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;

namespace Rogue.REditor
{
    public static class JsonSerializationLib
    {
        #region Serialized Properties

        public static Action<SerializedProperty> onPasted;

        public static void CopyValue<T>(this SerializedProperty property, T value, string reason) where T : class
        {
            var json = JsonConvert.SerializeObject(value, GetSerializationSettings(value));
            SetValueFromJson(property, json, typeof(T), reason);
        }

        public static string GetJson(this SerializedProperty property)
        {
            var value = property.GetValue();
            return JsonConvert.SerializeObject(value, GetSerializationSettings(value));
        }

        public static void SetValueFromJson(this SerializedProperty property, string json, Type valueType, string reason)
        {
            property.PrepareForChanges(reason);

            var currentValue = property.GetValue();
            var prePasteData = currentValue is IPasteSerializationFix prePasteFix ? prePasteFix.GetPrePasteData() : null;

            var deserializedObject = JsonConvert.DeserializeObject(json, valueType, GetSerializationSettings(null));

            if (deserializedObject is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(prePasteData);

            property.SetValue(deserializedObject);
            property.ApplyDirectChanges();

            onPasted?.Invoke(property);
        }

        #endregion

        #region Direct Object Manipulation

        public static T CreateCopy<T>(T source)
        {
            var json = GetJson(source);
            var targetType = source.GetType();
            return (T) JsonConvert.DeserializeObject(json, targetType, GetSerializationSettings(null));
        }

        public static void CopyValues(object source, object target, bool fixPasting)
        {
            var json = GetJson(source);
            SetValueFromJson(target, json, fixPasting);
        }

        public static string GetJson(object target) => JsonConvert.SerializeObject(target, GetSerializationSettings(target));

        public static void SetValueFromJson(object target, string json, bool fixPasting = true)
        {
            var prePasteData = fixPasting && target is IPasteSerializationFix prePasteFix ? prePasteFix.GetPrePasteData() : null;

            JsonConvert.PopulateObject(json, target, GetSerializationSettings(null));

            if (fixPasting && target is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(prePasteData);
        }

        // DO NOT USE WITH UNITY.OBJECT!
        public static object GetFromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject(json, GetSerializationSettings(null));

            if (obj is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(null);

            return obj;
        }

        public static T GetFromJson<T>(string json) where T : class
        {
            var obj = JsonConvert.DeserializeObject<T>(json, GetSerializationSettings(null));

            if (obj is IPasteSerializationFix pasteFix)
                pasteFix.FixSerializationPostPaste(null);

            return obj;
        }

        // DO NOT USE WITH UNITY.OBJECTS!
        public static List<object> GetObjectListFromJson(string json)
        {
            var settings = GetSerializationSettings(null);

            var jArray = JsonConvert.DeserializeObject(json, settings) as JArray;
            if (jArray == null) return null;

            var serializer = JsonSerializer.Create(settings);
            var entries = jArray.Select(token => token.ToObject<object>(serializer)).ToList();

            return entries;
        }

        #endregion

        private static JsonSerializerSettings GetSerializationSettings(object rootObj)
            => GetSerializationSettings(rootObj.GetType());

        private static JsonSerializerSettings GetSerializationSettings(Type rootType) => new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects,

            ContractResolver = new SerializeFieldContractResolver(),
            Converters = { new UnityObjectConverter(rootType), new AnimationCurveConverter() },
        };
    }

    public interface IPasteSerializationFix
    {
        public object GetPrePasteData() => null;
        public void FixSerializationPostPaste(object prePasteData);
    }
}