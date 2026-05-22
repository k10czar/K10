using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace K10.EventSystem
{
    public class PriorityEventSlot : IEventRegister<PriorityEventSlot>, IEventTrigger, ICustomDisposableKill
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

            if (!HasListeners) return;

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

        public void Clear()
        {
            if (listeners == null) return;

            ListPool<IEventTrigger<PriorityEventSlot>>.Release(listeners);
            listeners = null;
        }

        private void TryReleaseListeners()
        {
            if (listeners is not { Count: 0 }) return;
            Clear();
        }

        #region Register / Unregister Interface

        public void Register(IEventTrigger<PriorityEventSlot> listener)
        {
            if (killed || listener == null) return;

            listeners ??= ListPool<IEventTrigger<PriorityEventSlot>>.Get();
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

        public void Register(IEventTrigger listener)
        {
            if (killed || listener == null) return;
            Register(listener.Trigger);
        }

        public bool Unregister(IEventTrigger listener) => !killed && Unregister(listener.Trigger);

        public ActionCapsule<PriorityEventSlot> Register(Action act) => _ = new ActionCapsule<PriorityEventSlot>(act, this);
        public ActionCapsule<PriorityEventSlot> Register(Action<PriorityEventSlot> act) => _ = new ActionCapsule<PriorityEventSlot>(act, this);

        public bool Unregister(Action act) => Unregister(new ActionCapsule<PriorityEventSlot>(act, true));
        public bool Unregister(Action<PriorityEventSlot> act) => Unregister(new ActionCapsule<PriorityEventSlot>(act, true));

        #endregion

        public override string ToString() => $"[PrioritySlot:{EventsCount}]";
    }

    public class PriorityEventSlot<T> : IEventRegister<PriorityEventSlot<T>,T>, IEventTrigger<T>, ICustomDisposableKill
    {
        private bool killed = false;

        private List<IEventTrigger<PriorityEventSlot<T>,T>> listeners;

        public bool IsValid => !killed;
        public int EventsCount => listeners?.Count ?? 0;
        public bool HasListeners => EventsCount > 0;

        private bool IsTriggering => triggerIndex >= 0;

        private int triggerIndex = -1;
        private bool stopRequested;
        private bool removedCurrent;

        public void Trigger(T t)
        {
            if (killed) { Debug.LogError("Cannot Trigger killed PrioritySlot"); return; }
            if (IsTriggering) { Debug.LogError("PrioritySlot already triggering!"); return; }

            if (!HasListeners) return;

            stopRequested = false;

            for (triggerIndex = listeners.Count - 1; triggerIndex >= 0; triggerIndex--)
            {
                removedCurrent = false;
                var listener = listeners[triggerIndex];

                try
                {
                    if (listener.IsValid) listener.Trigger(this, t);
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

        public void Clear()
        {
            if (listeners == null) return;

            ListPool<IEventTrigger<PriorityEventSlot<T>,T>>.Release(listeners);
            listeners = null;
        }

        private void TryReleaseListeners()
        {
            if (listeners is not { Count: 0 }) return;
            Clear();
        }

        #region Register / Unregister Interface

        public void Register(IEventTrigger<PriorityEventSlot<T>,T> listener)
        {
            if (killed || listener == null) return;

            listeners ??= ListPool<IEventTrigger<PriorityEventSlot<T>,T>>.Get();
            listeners.Add(listener);
        }

        public bool Unregister(IEventTrigger<PriorityEventSlot<T>,T> listener)
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

        public void Register(IEventTrigger listener)
        {
            if (killed || listener == null) return;
            Register(listener.Trigger);
        }

        public bool Unregister(IEventTrigger listener) => !killed && Unregister(listener.Trigger);

        public void Register(IEventTrigger<PriorityEventSlot<T>> listener)
        {
            if (killed || listener == null) return;
            Register(listener.Trigger);
        }

        public bool Unregister(IEventTrigger<PriorityEventSlot<T>> listener) => !killed && Unregister(listener.Trigger);

        public ActionCapsule<PriorityEventSlot<T>,T> Register(Action act) => _ = new ActionCapsule<PriorityEventSlot<T>,T>(act, this);
        public ActionCapsule<PriorityEventSlot<T>,T> Register(Action<T> act) => _ = new ActionCapsule<PriorityEventSlot<T>,T>(act, this);
        public ActionCapsule<PriorityEventSlot<T>,T> Register(Action<PriorityEventSlot<T>> act) => _ = new ActionCapsule<PriorityEventSlot<T>,T>(act, this);
        public ActionCapsule<PriorityEventSlot<T>,T> Register(Action<PriorityEventSlot<T>,T> act) => _ = new ActionCapsule<PriorityEventSlot<T>,T>(act, this);

        public IFilteredActionCapsule RegisterFiltered(Action act) => new FilteredActionCapsule<PriorityEventSlot<T>,T>(act, this);
        public IFilteredActionCapsule RegisterFiltered(Action<T> act) => new FilteredActionCapsule<PriorityEventSlot<T>,T>(act, this);
        public IFilteredActionCapsule RegisterFiltered(Action<PriorityEventSlot<T>> act) => new FilteredActionCapsule<PriorityEventSlot<T>,T>(act, this);
        public IFilteredActionCapsule RegisterFiltered(Action<PriorityEventSlot<T>,T> act) => new FilteredActionCapsule<PriorityEventSlot<T>,T>(act, this);

        public bool Unregister(Action act) => Unregister(new ActionCapsule<PriorityEventSlot<T>,T>(act, true));
        public bool Unregister(Action<T> act) => Unregister(new ActionCapsule<PriorityEventSlot<T>,T>(act, true));
        public bool Unregister(Action<PriorityEventSlot<T>> act) => Unregister(new ActionCapsule<PriorityEventSlot<T>,T>(act, true));
        public bool Unregister(Action<PriorityEventSlot<T>,T> act) => Unregister(new ActionCapsule<PriorityEventSlot<T>,T>(act, true));

        #endregion

        public override string ToString() => $"[PrioritySlot<{typeof(T)}>:{EventsCount}]";
    }
}