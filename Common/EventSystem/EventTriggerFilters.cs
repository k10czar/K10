using System;
using UnityEngine;

namespace K10.EventSystem
{
    public class EventTriggerFilter<T> : IEventTrigger<T>, IEquatable<EventTriggerFilter<T>>, IEquatable<ActionCapsule>, IVoidable
    {
        private readonly IEventTrigger target;
        private readonly IEventRegister<T> observed;

        public EventTriggerFilter(IEventTrigger target)
        {
            this.target = target;
            Debug.Assert(target != null, "Registering filter for null Action capsule!");
        }

        public EventTriggerFilter(IEventTrigger target, IEventRegister<T> observed)
        {
            this.target = target;
            this.observed = observed;

            Debug.Assert(target != null, "Registering filter for null Action capsule!");

            observed.Register(this);
        }

        [HideInCallstack]
        public void Trigger(T t) => target.Trigger();
        public bool IsValid { get; private set; } = true;

        public void Void()
        {
            if (IsValid) observed?.Unregister(this);
            IsValid = false;
        }

        public bool Equals(EventTriggerFilter<T> other)
            => GetHashCode() == other?.GetHashCode() && target.Equals(other.target);

        public bool Equals(ActionCapsule other)
            => GetHashCode() == other?.GetHashCode() && target.Equals(other);

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (GetHashCode() != other.GetHashCode()) return false;

            return other switch
            {
                IEventTrigger otherTarget => target.Equals(otherTarget),
                EventTriggerFilter<T> otherFilter => Equals(otherFilter),
                _ => false
            };
        }

        public override int GetHashCode() => target.GetHashCode();
    }

}