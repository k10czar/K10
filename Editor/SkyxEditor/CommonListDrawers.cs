using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class CommonListDrawers
    {
        public static void OnNewPoint(SerializedProperty newElement, int mask, string pointName = "Point")
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Create at Click"), false, () => EditorUtils.RunOnSceneClick(hit => CreatePointAtHit(newElement, hit, pointName), mask));
            menu.AddItem(new GUIContent("Leave Null"), false, () => { newElement.objectReferenceValue = null; newElement.Apply(); });

            EditorUtils.RunOnSceneOnce(menu.ShowAsContext);
        }

        private static void CreatePointAtHit(SerializedProperty newElement, RaycastHit hit, string pointName)
        {
            var root = ((Component) newElement.serializedObject.targetObject).gameObject;
            Undo.RegisterCompleteObjectUndo(root, "Create child spawnPoint");

            newElement.ExtractArrayElementInfo(out _, out var index);

            var newObj = new GameObject($"{root.name}.{pointName}{index + 1}");
            newObj.SetActive(false);
            var newPoint = newObj.transform;
            newPoint.SetParent(root.transform);
            newPoint.position = hit.point;

            Undo.RegisterCreatedObjectUndo(newObj, "Create Child Object");

            newElement.objectReferenceValue = newPoint;
            newElement.Apply();

            Selection.activeObject = newObj;
        }
    }
}