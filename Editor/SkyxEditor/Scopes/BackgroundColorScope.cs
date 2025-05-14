using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class BackgroundColorScope : IDisposable
    {
        #region Interface

        public static BackgroundColorScope Set(EColor color)
        {
            var scope = pool.Get();

            scope.prevColor = GUI.backgroundColor;
            GUI.backgroundColor = color.Get();

            return scope;
        }

        public static BackgroundColorScope Set(Color backgroundColor)
        {
            var scope = pool.Get();

            scope.prevColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            return scope;
        }

        public static BackgroundColorScope Set(string htmlColor)
        {
            var scope = pool.Get();

            scope.prevColor = GUI.backgroundColor;

            if (ColorUtility.TryParseHtmlString(htmlColor, out var color))
                GUI.backgroundColor = color;

            return scope;
        }

        #endregion

        #region Instance Info

        private Color prevColor;

        public void Dispose()
        {
            GUI.backgroundColor = prevColor;
            pool.Release(this);
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<BackgroundColorScope> pool = new(CreateScope);
        private static BackgroundColorScope CreateScope() => new();

        #endregion
    }
}