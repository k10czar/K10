using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rogue.Helpers
{
    public static class NumbersLib
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddCircular(this int value, int max = 2) => (value + 1) % max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RemoveCircular(this int value, int max = 2) => (value - 1 + max) % max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddCircular(this int value, int delta, int max) => ((value + delta) % max + max) % max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddClamped(this int value, int delta, int min, int max) => Mathf.Clamp(value + delta, min, max);

        public static float Remap(this float from, float fromMin, float fromMax, float toMin,  float toMax)
        {
            var fromAbs  =  from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        public static float RemapFromNormalized(this float from, float toMin, float toMax) => from.Remap(0f, 1f, toMin, toMax);

        public static float Normalize(this float value, float min, float max, bool clamp = true)
        {
            var normalized = (value - min) / (max - min);
            return clamp ? Mathf.Clamp01(normalized) : normalized;
        }

        public static float FixPrecision(this float value, int precision) => Mathf.Round(value * precision) / precision;

        public static bool IsRounded(this float target)
        {
            var remainder = Mathf.Abs(target % 1);
            return remainder is < 0.00005f or > 1f - 0.00005f;
        }

        public static bool AreRounded(params float[] values) => values.All(value => value.IsRounded());

        public static bool IsPowerOfTwo(this int num) => num > 0 && (num & (num - 1)) == 0;

        public static float SquareSampling(float v00, float v01, float v10, float v11, float xLerp, float yLerp)
        {
            var top = Mathf.Lerp(v00, v10, xLerp);
            var bottom = Mathf.Lerp(v01, v11, xLerp);

            return Mathf.Lerp(top, bottom, yLerp);
        }

        #region Angles

        public static readonly float[] CardinalAngles = { 0, 90, -90, 180 };
        public static readonly float[] OrthogonalAngles = { 0, 45, -45, 90, -90, 135, -135, 180, };
        public static readonly float[] UnsignedOrthogonalAngles = { 0, 45, 90, 135, 180, };

        // Adjusts angle to [-180, 180]
        public static float AdjustAngle(this float angle)
        {
            angle %= 360f;

            return angle switch
            {
                > 180 => angle - 360,
                < -180 => angle + 360,
                _ => angle
            };
        }

        public static float GetClosestOrthogonalAngle(this float angle)
        {
            var closest = float.MaxValue;
            var minAngleDiff = float.MaxValue;

            foreach (var candidate in OrthogonalAngles)
            {
                var diff = Mathf.Abs(candidate - angle);
                if (diff > minAngleDiff) continue;

                minAngleDiff = diff;
                closest = candidate;
            }

            return closest;
        }

        public static bool IsCardinalAngle(this float angle) => IsOneOfTargetAngles(angle, CardinalAngles);
        public static bool IsOrthogonalAngle(this float angle) => IsOneOfTargetAngles(angle, OrthogonalAngles);

        private static bool IsOneOfTargetAngles(this float angle, float[] candidates)
        {
            var adjusted = AdjustAngle(angle);

            foreach (var candidate in candidates)
            {
                if (Mathf.Approximately(candidate, adjusted))
                    return true;
            }

            return false;
        }

        public static bool IsAngleInRange(this float angle, float targetAngle, float maxAngleDiff)
            => Mathf.Abs(Mathf.DeltaAngle(angle, targetAngle)) <= maxAngleDiff;

        #endregion

        #region Hashing

        private const uint MagicNumber = 0x9e3779b9; // Golden ratio constant

        public static int StableHash(params object[] values) => GenerateStableHash(values);

        public static int GenerateStableHash(IReadOnlyList<object> values)
        {
            if (values == null || values.Count == 0)
                return 0;

            uint hash = MagicNumber;

            foreach (var value in values)
            {
                uint stableInt = GetStableInt(value);
                hash ^= stableInt + MagicNumber + (hash << 6) + (hash >> 2);
            }

            unchecked
            {
                return (int)hash;
            }
        }

        private static uint GetStableInt(object value)
        {
            if (value == null) return 0;

            return value switch
            {
                int i => (uint)i,
                bool b => b ? 1u : 0u,
                string s => StableStringHash(s),
                Enum e => unchecked((uint)Convert.ToInt64(e)),
                _ => throw new ArgumentException($"Unsupported type: {value.GetType().Name}. Only int, string, and bool are supported.")
            };
        }

        private static uint StableStringHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            var hash = MagicNumber;
            foreach (var c in input)
                hash ^= c + MagicNumber + (hash << 6) + (hash >> 2);

            return hash;
        }

        #endregion
    }
}