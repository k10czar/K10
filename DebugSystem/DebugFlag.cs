using UnityEngine;

namespace K10.DebugSystem
{
    public class DebugFlag
    {
        private readonly string flag;
        private readonly string onDebug;
        private readonly string offDebug;

        public void Toggle()
        {
            K10DebugSystem.ToggleFlag(flag);

            if (onDebug != null) Debug.Log(IsEnabled ? onDebug : offDebug);
            else Debug.Log($"DebugFlag <b>{flag}</b> set to {IsEnabled}");
        }

        public bool IsEnabled => K10DebugSystem.CanDebugFlag(flag);
        public static implicit operator bool(DebugFlag flag) => flag.IsEnabled;

        public override string ToString() => flag;

        public DebugFlag(string flag, string onDebug = null, string offDebug = null)
        {
            this.flag = flag;
            this.onDebug = onDebug;
            this.offDebug = offDebug;
        }
    }
}