using System;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class EditorUtils
    {
        public static void RunDelayedOnce(Action action)
        {
            EditorApplication.CallbackFunction wrapper = null;

            wrapper = () =>
            {
                action();
                EditorApplication.delayCall -= wrapper;
            };

            EditorApplication.delayCall += wrapper;
        }

        public static void RunOnSceneOnce(Action<SceneView> action)
        {
            Action<SceneView> wrapper = null;

            wrapper = sceneView =>
            {
                action(sceneView);
                SceneView.duringSceneGui -= wrapper;
            };

            SceneView.duringSceneGui += wrapper;
        }

        public static void RunOnSceneOnce(Action action)
        {
            Action<SceneView> wrapper = null;

            wrapper = _ =>
            {
                action();
                SceneView.duringSceneGui -= wrapper;
            };

            SceneView.duringSceneGui += wrapper;
        }

        #region Cache Clears

        [MenuItem("Disyphus/Clear Editor Caches/Clear SerializedReferences Drawers")]
        private static void ClearSerializedRefDrawers()
        {
            SerializedRefLib.customDrawerCache.Clear();
        }

        #endregion
    }
}