using System;
using System.Collections.Generic;
using UnityEngine;

namespace K10.EventSystem
{
    public class PriorityEventSlot
        : IEventRegister<PriorityEventSlot>, IEventTrigger, ICustomDisposableKill
    {
        private bool killed = false;
        private List<IEventTrigger<PriorityEventSlot>> listeners;

        public bool IsValid => !killed;
        public int EventsCount => listeners?.Count ?? 0;
        public bool HasListeners => EventsCount > 0;

        private bool IsTriggering => triggerIndex >= 0;

        private int triggerIndex = -1;
        private bool stopRequested;
        private bool removedCurrent;

        public void Trigger()
        {
            if (killed) { Debug.LogError("Cannot Trigger killed PrioritySlot"); return; }
            if (IsTriggering) { Debug.LogError("PrioritySlot already triggering!"); return; }

            if (listeners == null) return;

            stopRequested = false;

            for (triggerIndex = listeners.Count - 1; triggerIndex >= 0; triggerIndex--)
            {
                removedCurrent = false;
                var listener = listeners[triggerIndex];

                try
                {
                    if (listener.IsValid) listener.Trigger(this);
                    if (!removedCurrent && !listener.IsValid) listeners.RemoveAt(triggerIndex);
                    if (stopRequested) break;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            triggerIndex = -1;
            TryReleaseListeners();
        }

        public void UseEvent()
        {
            Debug.Assert(IsTriggering, "Trying to stop triggering non triggering priority slot!");
            stopRequested = true;
        }

        public void Kill()
        {
            killed = true;
            Clear();
        }

        public void Clear() => ObjectPool<List<IEventTrigger<PriorityEventSlot>>>.Return(ref listeners);

        private void TryReleaseListeners()
        {
            if (listeners == null || listeners.Count != 0) return;
            Clear();
        }

        public void Register(IEventTrigger<PriorityEventSlot> listener)
        {
            if (killed || listener == null) return;

            listeners ??= ObjectPool<List<IEventTrigger<PriorityEventSlot>>>.Request();
            listeners.Add(listener);
        }

        public bool Unregister(IEventTrigger<PriorityEventSlot> listener)
        {
            if (killed || listeners == null) return false;

            var index = listeners.IndexOf(listener);
            if (index < 0) return false;

            listeners.RemoveAt(index);

            if (IsTriggering)
            {
                if (index < triggerIndex) triggerIndex--;
                else if (index == triggerIndex) removedCurrent = true;
            }
            else TryReleaseListeners();

            return true;
        }

        public void Register(IEventTrigger listener) => _ = new EventTriggerFilter<PriorityEventSlot>(listener, this);
        public bool Unregister(IEventTrigger listener) => Unregister(new EventTriggerFilter<PriorityEventSlot>(listener));

        public override string ToString() => $"[PrioritySlot:{EventsCount}]";

    }
}