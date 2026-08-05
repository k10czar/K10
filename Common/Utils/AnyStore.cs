using System;
using System.Collections.Generic;
using UnityEngine;

namespace K10.Common
{
    public class AnyStore<T>
    {
        private readonly Dictionary<T, object> dict = new();

        public IEnumerable<T> Keys => dict.Keys;

        public U Get<U>(T id)
        {
            Debug.Assert(dict.ContainsKey(id), $"Trying to get nonexistent stored data for {typeof(U)} ({id})!");
            return (U) dict[id];
        }

        public U GetOrCreate<U>(T id) where U : new()
        {
            if (TryGet(id, out U value)) return value;

            var newValue = new U();
            Store(id, newValue);
            return newValue;
        }

        public U GetOrCreate<U>(T id, Func<U> factory)
        {
            if (TryGet(id, out U value)) return value;

            var newValue = factory();
            Store(id, newValue);
            return newValue;
        }

        public bool TryGet<U>(T id, out U data)
        {
            if (dict.TryGetValue(id, out var value))
            {
                data = (U) value;
                return true;
            }

            data = default;
            return false;
        }

        public bool Has(T id) => dict.ContainsKey(id);

        public void Store<U>(T id, U data, bool replace = false)
        {
            Debug.Assert(replace || !dict.ContainsKey(id), $"Replacing: {id}");
            dict[id] = data;
        }

        public void Release(T id) => dict.Remove(id);

        public void Clear() => dict.Clear();
    }
}