using System;
using UnityEngine;

namespace K10.EventSystem
{
    public static class ActionWrapperExtensions
    {
        [HideInCallstack]
        public static Action<T> Wrap<T>(this Action callback) => _ => callback();

        [HideInCallstack]
        public static Action<T,K> Wrap<T,K>(this Action<T> callback) => (t, _) => callback(t);
        [HideInCallstack]
        public static Action<T,K> Wrap<T,K>(this Action<K> callback) => (_, k) => callback(k);
        [HideInCallstack]
        public static Action<T,K> Wrap<T,K>(this Action callback) => (_, _) => callback();


        [HideInCallstack]
        public static Action<T,K,L> Wrap<T,K,L>(this Action<T,K> callback) => (t, k, _) => callback(t,k);
        [HideInCallstack]
        public static Action<T, K, L> Wrap<T, K, L>(this Action<T, L> callback) => (t, _, l) => callback(t,l);
        [HideInCallstack]
        public static Action<T,K,L> Wrap<T,K,L>(this Action<K,L> callback) => (_, k, l) => callback(k,l);
        [HideInCallstack]
        public static Action<T,K,L> Wrap<T,K,L>(this Action<T> callback) => (t, _, _) => callback(t);
        [HideInCallstack]
        public static Action<T,K,L> Wrap<T,K,L>(this Action<K> callback) => (_, k, _) => callback(k);
        [HideInCallstack]
        public static Action<T,K,L> Wrap<T,K,L>(this Action<L> callback) => (_, _, l) => callback(l);
        [HideInCallstack]
        public static Action<T,K,L> Wrap<T,K,L>(this Action callback) => (_, _, _) => callback();

        [HideInCallstack]
        public static Action Wrap(this Action<object[]> callback) => () => callback(Array.Empty<object>());
        [HideInCallstack]
        public static Action<T> Wrap<T>(this Action<object[]> callback) => t => callback(new object[] { t });
        [HideInCallstack]
        public static Action<T,K> Wrap<T,K>(this Action<object[]> callback) => (t, k) => callback(new object[] { t, k });
        [HideInCallstack]
        public static Action<T,K,L> Wrap<T,K,L>(this Action<object[]> callback) => (t, k, l) => callback(new object[] { t, k, l });
    }
}