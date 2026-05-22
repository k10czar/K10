using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Rogue.REditor
{
    public class EditorIndentScope : IDisposable
    {
        #region Interface

        public static EditorIndentScope Set(int indentLevel)
        {
            var scope = pool.Get();

            scope.prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;

            return scope;
        }

        public static EditorIndentScope Increment(int delta)
        {
            var scope = pool.Get();

            scope.prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel += delta;

            return scope;
        }

        #endregion

        #region Instance Info

        private int prevIndent;

        public void Dispose()
        {
            EditorGUI.indentLevel = prevIndent;
            pool.Release(this);
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<EditorIndentScope> pool = new(CreateScope);
        private static EditorIndentScope CreateScope() => new();

        #endregion
    }
}