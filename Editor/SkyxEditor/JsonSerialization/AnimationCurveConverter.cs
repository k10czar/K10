using System;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rogue.REditor
{
    public class AnimationCurveConverter : JsonConverter<AnimationCurve>
    {
        public override void WriteJson(JsonWriter writer, AnimationCurve value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("preWrapMode");
            writer.WriteValue(value.preWrapMode.ToString());

            writer.WritePropertyName("postWrapMode");
            writer.WriteValue(value.postWrapMode.ToString());

            writer.WritePropertyName("keys");
            writer.WriteStartArray();
            foreach (var key in value.keys)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("time"); writer.WriteValue(key.time);
                writer.WritePropertyName("value"); writer.WriteValue(key.value);
                writer.WritePropertyName("inTangent"); writer.WriteValue(key.inTangent);
                writer.WritePropertyName("outTangent"); writer.WriteValue(key.outTangent);
                writer.WritePropertyName("inWeight"); writer.WriteValue(key.inWeight);
                writer.WritePropertyName("outWeight"); writer.WriteValue(key.outWeight);
                writer.WritePropertyName("weightedMode"); writer.WriteValue((int)key.weightedMode);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        public override AnimationCurve ReadJson(JsonReader reader, Type objectType, AnimationCurve existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject obj = JObject.Load(reader);
            var curve = new AnimationCurve();

            var keysArray = obj["keys"] as JArray;
            if (keysArray != null)
            {
                foreach (var k in keysArray)
                {
                    var kf = new Keyframe(
                        k["time"].Value<float>(),
                        k["value"].Value<float>(),
                        k["inTangent"].Value<float>(),
                        k["outTangent"].Value<float>()
                    );

                    if (k["inWeight"] != null) kf.inWeight = k["inWeight"].Value<float>();
                    if (k["outWeight"] != null) kf.outWeight = k["outWeight"].Value<float>();
                    if (k["weightedMode"] != null) kf.weightedMode = (WeightedMode)k["weightedMode"].Value<int>();

                    curve.AddKey(kf);
                }
            }

            if (obj["preWrapMode"] != null)
                curve.preWrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), obj["preWrapMode"].Value<string>());
            if (obj["postWrapMode"] != null)
                curve.postWrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), obj["postWrapMode"].Value<string>());

            return curve;
        }
    }
}