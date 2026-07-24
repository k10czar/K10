using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(LocalPositionAttribute))]
    public class LocalPositionPropertyDrawer : PropertyDrawer
    {
        private const string AnchorName = "[HelpingAnchor]";

        private static void OpenMenuPicker(SerializedProperty property, Transform sourceTransform, int validMask)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Get Existing Anchor"), false, () => SelectExistingAnchor(sourceTransform));
            menu.AddItem(new GUIContent("Create anchor at current position"), false, () => CreateSelectingAnchor(sourceTransform, property.vector3Value));
            menu.AddItem(new GUIContent("Create anchor at Click"), false, () => EditorUtils.RunOnSceneClick(hit => CreateSelectingAnchor(sourceTransform, sourceTransform.InverseTransformPoint(hit.point)), validMask));

            menu.ShowAsContext();
        }

        private static void SelectExistingAnchor(Transform sourceTransform)
        {
            var anchor = sourceTransform.Find(AnchorName);
            if (anchor == null)
            {
                Debug.LogError($"Cannot find anchor {AnchorName}");
                return;
            }

            EditorUtils.SetFocusedInspectorLock(true);
            Selection.activeObject = anchor;
        }

        private static void CreateSelectingAnchor(Transform sourceTransform, Vector3 localPosition)
        {
            EditorUtils.SetFocusedInspectorLock(true);

            var anchor = new GameObject("[HelpingAnchor]")
            {
                transform =
                {
                    parent = sourceTransform,
                    localPosition = localPosition,
                    localRotation = Quaternion.identity
                },
                hideFlags = HideFlags.DontSave
            };

            Selection.activeObject = anchor;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            rect.AdjustToLine();

            if (!Application.isPlaying)
            {
                var sourceTransform = ((Component) property.serializedObject.targetObject).transform;
                var selectingAnchor = Selection.activeTransform != null &&
                                      Selection.activeTransform.parent == sourceTransform &&
                                      Selection.activeTransform.gameObject.hideFlags == HideFlags.DontSave;

                if (selectingAnchor)
                {
                    if (SkyxGUI.MiniButton(ref rect, "📋", EColor.Warning, "Copy local position from selected object", true))
                    {
                        property.vector3Value = Selection.activeTransform.localPosition;
                        property.Apply();
                    }

                    if (SkyxGUI.MiniButton(ref rect, "📌", EColor.Support, "Set local position on selected object", true))
                        Selection.activeTransform.localPosition = property.vector3Value;

                    if (SkyxGUI.MiniButton(ref rect, "❌", EColor.Support, "Delete helping Anchor", true))
                        Object.DestroyImmediate(Selection.activeObject);
                }
                else
                {
                    if (SkyxGUI.MiniButton(ref rect, "⊙", EColor.Support, "Create helping anchor", true))
                        OpenMenuPicker(property, sourceTransform, ((LocalPositionAttribute)attribute).validMask);
                }
            }

            EditorGUI.PropertyField(rect, property, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }
}