using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Rogue.REditor
{
    public class IgnoreDisabledGUIScope : IDisposable
    {
        #region Interface

        public static IgnoreDisabledGUIScope Start()
        {
            var scope = pool.Get();

            scope.prevDisabledCount = 0;

            while (!GUI.enabled)
            {
                scope.prevDisabledCount++;
                EditorGUI.EndDisabledGroup();
            }

            return scope;
        }

        #endregion

        #region Instance Info

        private int prevDisabledCount;

        public void Dispose()
        {
            for (var i = 0; i < prevDisabledCount; i++)
                EditorGUI.BeginDisabledGroup(true);

            pool.Release(this);
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<IgnoreDisabledGUIScope> pool = new(CreateScope);
        private static IgnoreDisabledGUIScope CreateScope() => new();

        #endregion
    }
}