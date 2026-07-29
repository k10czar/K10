using System;
using System.Runtime.CompilerServices;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.REditor
{
    public class UnityObjectConverter : JsonConverter
    {
        private bool skipRoot;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsUnityType(Type type) => typeof(Object).IsAssignableFrom(type);

        public override bool CanConvert(Type objectType)
        {
            if (skipRoot)
            {
                Debug.Assert(IsUnityType(objectType), "Skipping non-Unity type root!");
                skipRoot = false;
                return false;
            }

            return IsUnityType(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonObject = new JObject { { "instanceID", ((Object) value)!.GetInstanceID() } };
            jsonObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var instanceID = jsonObject["instanceID"]!.ToObject<int>();

            var unityObject = EditorUtility.EntityIdToObject(instanceID);
            if (unityObject != null) return unityObject;

            if (instanceID != 0) Debug.LogError($"Unity object with instanceID {instanceID} not found.");

            return null;
        }

        public UnityObjectConverter(Type rootType) => skipRoot = rootType != null && IsUnityType(rootType);
    }
}