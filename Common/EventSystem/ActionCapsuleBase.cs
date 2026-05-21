using System;

namespace K10.EventSystem
{
    public class ActionCapsuleBase : IEquatable<ActionCapsuleBase>, IVoidable
    {
        private readonly object keyAction;

        public bool IsValid { get; protected set; }

        public virtual void Void() => IsValid = false;

        public ActionCapsuleBase(object keyAction)
        {
            this.keyAction = keyAction;
            IsValid = keyAction != null;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (GetHashCode() != other.GetHashCode()) return false;

            if (other is ActionCapsuleBase cap)
                return keyAction?.Equals(cap.keyAction) ?? cap.keyAction == null;

            return false;
        }

        public bool Equals(ActionCapsuleBase other)
        {
            if (GetHashCode() != other?.GetHashCode()) return false;
            return keyAction?.Equals(other.keyAction) ?? other.keyAction == null;
        }

        public override int GetHashCode() => keyAction?.GetHashCode() ?? 0;
    }
}