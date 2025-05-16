using System;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class PropertyContextMenu
    {
        private static readonly JsonSerializerSettings serializerSettings;

        private static Type copiedType;
        private static string copiedDisplayInfo;

        private static Type selectedType;
        private static string selectedDisplayInfo;
        private static SerializedProperty selectedProperty;

        public static void Open(SerializedProperty property)
        {
            selectedProperty = property;
            selectedType = property.GetValue().GetType();
            selectedDisplayInfo = $"{property.displayName} ({selectedType})";

            var menu = new GenericMenu();

            menu.AddDisabledItem(new GUIContent(selectedDisplayInfo));
            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Copy"), false, OnCopy);

            if (HasPasteData)
            {
                if (IsValidPasteTarget) menu.AddItem(new GUIContent("Paste"), false, OnPaste);
                else menu.AddDisabledItem(new GUIContent($"Paste | {copiedType}"));
            }

            if (property.IsArrayEntry())
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Insert Array Element Above"), false, OnInsertElementAbove);
                menu.AddItem(new GUIContent("Insert Array Element Below"), false, OnInsertElementBelow);
                menu.AddItem(new GUIContent("Duplicate Array Element"), false, OnDuplicateElement);
                menu.AddItem(new GUIContent("Delete Array Element"), false, OnDeleteElement);
            }
            else if (property.propertyType is SerializedPropertyType.ObjectReference)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Copy System Path"), false, OnCopySystemPath);
                menu.AddItem(new GUIContent("Copy GUID"), false, OnCopyGUID);
            }

            menu.ShowAsContext();
        }

        private static void OnCopy()
        {
            var json = selectedProperty.GetJson();

            if (string.IsNullOrEmpty(json) || json == "{}" || !IsValidJson(json))
            {
                copiedType = null;
                copiedDisplayInfo = null;
                EditorGUIUtility.systemCopyBuffer = "";

                Debug.LogError($"Copying <b>{selectedType}</b> is a not supported!");
            }
            else
            {
                copiedType = selectedType;
                copiedDisplayInfo = selectedDisplayInfo;
                EditorGUIUtility.systemCopyBuffer = json;
            }
        }

        private static bool HasPasteData => !string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer) && IsValidJson(EditorGUIUtility.systemCopyBuffer);
        private static bool IsValidPasteTarget => copiedType == selectedType;
        private static bool CanPaste => HasPasteData && IsValidPasteTarget;

        private static bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException) { return false; }
        }

        private static void OnPaste()
        {
            if (!CanPaste) return;
            selectedProperty.SetValueFromJson(EditorGUIUtility.systemCopyBuffer, copiedType, $"Pasted data from clipboard to {selectedDisplayInfo}");
        }

        private static void OnInsertElementAbove()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.InsertArrayElementAtIndex(index);
            parent.Apply();

            ResetElement(parent.GetArrayElementAtIndex(index), $"Insert element above {selectedDisplayInfo}");
        }

        private static void OnInsertElementBelow()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.InsertArrayElementAtIndex(index);
            parent.Apply();

            ResetElement(parent.GetArrayElementAtIndex(index + 1), $"Insert element below {selectedDisplayInfo}");
        }

        private static void ResetElement(SerializedProperty newElement, string reason)
        {
            if (newElement.IsManagedRef())
            {
                newElement.managedReferenceValue = null;
                newElement.Apply();

                EditorUtils.RunOnSceneOnce(() => SerializedRefLib.DrawTypePickerMenu(newElement));
            }
            else
            {
                selectedProperty.PrepareForChanges(reason);
                newElement.SetValue(selectedProperty.GenerateDefaultValue());
                selectedProperty.ApplyDirectChanges(reason);
            }
        }

        private static void OnDuplicateElement()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.InsertArrayElementAtIndex(index);
            parent.Apply();
        }

        private static void OnDeleteElement()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.DeleteArrayElementAtIndex(index);
            parent.Apply();
        }

        private static void OnCopySystemPath()
        {
        }

        private static void OnCopyGUID()
        {
        }
    }
}