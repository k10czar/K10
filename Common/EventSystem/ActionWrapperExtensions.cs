using System;

namespace K10.EventSystem
{
    public static class ActionWrapperExtensions
    {
        public static Action<T> Wrap<T>(this Action callback) => _ => callback();

        public static Action<T,K> Wrap<T,K>(this Action<T> callback) => (t, _) => callback(t);
        public static Action<T,K> Wrap<T,K>(this Action<K> callback) => (_, k) => callback(k);
        public static Action<T,K> Wrap<T,K>(this Action callback) => (_, _) => callback();


        public static Action<T,K,L> Wrap<T,K,L>(this Action<T,K> callback) => (t, k, _) => callback(t,k);
        public static Action<T, K, L> Wrap<T, K, L>(this Action<T, L> callback) => (t, _, l) => callback(t,l);
        public static Action<T,K,L> Wrap<T,K,L>(this Action<K,L> callback) => (_, k, l) => callback(k,l);
        public static Action<T,K,L> Wrap<T,K,L>(this Action<T> callback) => (t, _, _) => callback(t);
        public static Action<T,K,L> Wrap<T,K,L>(this Action<K> callback) => (_, k, _) => callback(k);
        public static Action<T,K,L> Wrap<T,K,L>(this Action<L> callback) => (_, _, l) => callback(l);
        public static Action<T,K,L> Wrap<T,K,L>(this Action callback) => (_, _, _) => callback();
    }
}