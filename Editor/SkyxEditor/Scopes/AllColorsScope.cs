using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class AllColorsScope : IDisposable
    {
        #region Interface

        public static AllColorsScope Set(EColor color)
        {
            var scope = pool.Get();

            scope.prevBackground = GUI.backgroundColor;
            scope.prevContent = GUI.contentColor;
            scope.prevColor = GUI.color;

            var newColor = color.Get();
            GUI.backgroundColor = newColor;
            GUI.contentColor = newColor;
            GUI.color = newColor;

            return scope;
        }

        public static AllColorsScope Set(Color newColor)
        {
            var scope = pool.Get();

            scope.prevBackground = GUI.backgroundColor;
            scope.prevContent = GUI.contentColor;
            scope.prevColor = GUI.color;

            GUI.backgroundColor = newColor;
            GUI.contentColor = newColor;
            GUI.color = newColor;

            return scope;
        }

        #endregion

        #region Instance Info

        private Color prevBackground;
        private Color prevContent;
        private Color prevColor;

        public void Dispose()
        {
            GUI.backgroundColor = prevBackground;
            GUI.contentColor = prevContent;
            GUI.color = prevColor;

            pool.Release(this);
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<AllColorsScope> pool = new(CreateScope);
        private static AllColorsScope CreateScope() => new();

        #endregion
    }
}