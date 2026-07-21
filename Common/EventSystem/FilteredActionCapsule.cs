using System;
using UnityEngine;

namespace K10.EventSystem
{
    public interface IFilteredActionCapsule
    {
        public void SetKeys(params object[] newKeys)
        {
            for (var index = 0; index < newKeys.Length; index++)
                SetKey(newKeys[index], index);
        }

        public void SetKey(object newKey, int index);
        public void RemoveKey(int index);
    }

    public class FilteredActionCapsule<T> : ActionCapsule<T>, IFilteredActionCapsule
    {
        [HideInCallstack]
        public override void Trigger(T t)
        {
            if (!IsValid) return;
            if (keyT.Item1 && !keyT.Item2.Equals(t)) return;

            callback(t);
        }

        public override void Void()
        {
            base.Void();
            RemoveKey(0);
        }

        #region Keys

        private (bool, object) keyT;

        public void SetKey(object newKey, int index)
        {
            Debug.Assert(index == 0, "Setting key on invalid slot!");

            keyT.Item1 = true;
            keyT.Item2 = newKey;
        }

        public void RemoveKey(int index)
        {
            Debug.Assert(index == 0, "Removing key from invalid slot!");

            keyT.Item1 = false;
            keyT.Item2 = null;
        }

        #endregion

        #region Constructors

        public FilteredActionCapsule(Action<T> callback, IEventRegister<T> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action callback, IEventRegister<T> observed) : base(callback, observed) {}

        #endregion
    }

    public class FilteredActionCapsule<T,K> : ActionCapsule<T,K>, IFilteredActionCapsule
    {
        [HideInCallstack]
        public override void Trigger(T t, K k)
        {
            if (!IsValid) return;
            if (keyT.Item1 && !keyT.Item2.Equals(t)) return;
            if (keyK.Item1 && !keyK.Item2.Equals(k)) return;

            callback(t,k);
        }

        public override void Void()
        {
            base.Void();
            RemoveKey(0);
            RemoveKey(1);
        }

        #region Keys

        private (bool, object) keyT;
        private (bool, object) keyK;

        public void SetKey(object newKey, int index)
        {
            Debug.Assert(index is >= 0 and < 2, "Removing key from invalid slot!");
            ref var entry = ref (index == 0 ? ref keyT : ref keyK);

            entry.Item1 = true;
            entry.Item2 = newKey;
        }

        public void RemoveKey(int index)
        {
            Debug.Assert(index is >= 0 and < 2, "Removing key from invalid slot!");
            ref var entry = ref (index == 0 ? ref keyT : ref keyK);

            entry.Item1 = false;
            entry.Item2 = null;
        }

        #endregion

        #region Constructors

        public FilteredActionCapsule(Action<T,K> callback, IEventRegister<T,K> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<T> callback, IEventRegister<T,K> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<K> callback, IEventRegister<T,K> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action callback, IEventRegister<T,K> observed) : base(callback, observed) {}

        #endregion
    }

    public class FilteredActionCapsule<T,K,L> : ActionCapsule<T,K,L>, IFilteredActionCapsule
    {
        [HideInCallstack]
        public override void Trigger(T t, K k, L l)
        {
            if (!IsValid) return;
            if (keyT.Item1 && !keyT.Item2.Equals(t)) return;
            if (keyK.Item1 && !keyK.Item2.Equals(k)) return;
            if (keyL.Item1 && !keyL.Item2.Equals(l)) return;

            callback(t,k,l);
        }

        public override void Void()
        {
            base.Void();
            RemoveKey(0);
            RemoveKey(1);
            RemoveKey(2);
        }

        #region Keys

        private (bool, object) keyT;
        private (bool, object) keyK;
        private (bool, object) keyL;

        public void SetKey(object newKey, int index)
        {
            Debug.Assert(index is >= 0 and < 3, "Removing key from invalid slot!");
            ref var entry = ref (index == 0 ? ref keyT : ref (index == 1 ? ref keyK : ref keyL));

            entry.Item1 = true;
            entry.Item2 = newKey;
        }

        public void RemoveKey(int index)
        {
            Debug.Assert(index is >= 0 and < 3, "Removing key from invalid slot!");
            ref var entry = ref (index == 0 ? ref keyT : ref (index == 1 ? ref keyK : ref keyL));

            entry.Item1 = false;
            entry.Item2 = null;
        }

        #endregion

        #region Constructors

        public FilteredActionCapsule(Action<T,K,L> callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<T,K> callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<T,L> callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<K,L> callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<T> callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<K> callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action<L> callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}
        public FilteredActionCapsule(Action callback, IEventRegister<T,K,L> observed) : base(callback, observed) {}

        #endregion
    }
}