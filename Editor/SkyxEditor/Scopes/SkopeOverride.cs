using System.Collections.Generic;
using System.Linq;
using Skyx.RuntimeEditor;
using UnityEditor;

namespace Rogue.REditor
{
    public class SkopeOverride
    {
        #region Static Interface

        public const string RemovedMsg = "OVERRIDE REMOVED";
        public const string ReplacedMsg = "OVERRIDE REPLACED";

        private static readonly Dictionary<(int, string), SkopeOverride> overrides = new();

        public static bool TryGetOverride(SerializedProperty target, out SkopeOverride skopeOverride)
            => overrides.TryGetValue(target.GetCacheID(), out skopeOverride);

        public static void SetOverrideData(SerializedProperty target, EColor color, bool disableInput, params SkopeButton[] buttons)
        {
            var appendOverride = color switch
            {
                EColor.Danger => RemovedMsg,
                EColor.Warning => ReplacedMsg,
                _ => string.Empty
            };

            overrides[target.GetCacheID()] = new SkopeOverride(color, appendOverride, disableInput, buttons.ToList());
        }

        public static void Release(SerializedProperty target) => overrides.Remove(target.GetCacheID());
        public static void Release((int, string) cacheID) => overrides.Remove(cacheID);

        public static void Release(int mainCacheID)
        {
            var keysToRemove = overrides.Keys.Where(k => k.Item1 == mainCacheID).ToList();
            foreach (var key in keysToRemove)
                overrides.Remove(key);
        }

        public static void Clear() => overrides.Clear();

        #endregion

        #region Override Data

        public readonly EColor color;
        public readonly string appendTitle;
        public readonly bool disableInput;
        public readonly List<SkopeButton> buttons;

        public bool ForcesInfo => !string.IsNullOrEmpty(appendTitle) || color is not EColor.Infer;
        public bool ForcesButtons => buttons != null;

        public SkopeOverride(EColor color, string appendTitle, bool disableInput, List<SkopeButton> buttons)
        {
            this.color = color;
            this.appendTitle = appendTitle;
            this.disableInput = disableInput;
            this.buttons = buttons;
        }

        #endregion
    }
}