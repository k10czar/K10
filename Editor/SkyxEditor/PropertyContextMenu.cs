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
        private static string copiedID;

        private static Type selectedType;
        private static string selectedDisplayInfo;
        private static SerializedProperty selectedProperty;
        private static Action<SerializedProperty> selectedElementSetup;

        public static void ContextGUI(ref Rect rect, SerializedProperty property, Action<SerializedProperty> newElementSetup = null)
        {
            var current = Event.current;
            if (!rect.Contains(current.mousePosition)) return;

            if (current.type == EventType.MouseDown && current.button == 1)
            {
                current.Use();
                Open(property, newElementSetup);
                return;
            }

            if (EditorGUIUtility.editingTextField || !current.control || current.type != EventType.KeyDown) return;

            if (current.keyCode == KeyCode.C)
            {
                TrackProperty(property, newElementSetup);
                OnCopy();
            }
            else if (current.keyCode == KeyCode.V)
            {
                TrackProperty(property, newElementSetup);

                if (!HasPasteData) Debug.LogError("No Paste Data!");
                else if (!IsValidPasteTarget) Debug.LogError($"Invalid Paste Target! {copiedType.Name}(Copied) != {selectedType.Name}(Target)");
                else OnPaste();
            }
            else if (current.keyCode == KeyCode.X)
            {
                if (!property.IsArrayEntry())
                {
                    Debug.LogWarning("Trying to cut a non-array entry!");
                    return;
                }

                TrackProperty(property, newElementSetup);
                OnCopy();

                EditorUtils.RunDelayedOnce(() => property.RemoveSelfFromArray());
            }
        }

        public static void Open(SerializedProperty property, Action<SerializedProperty> newElementSetup = null)
        {
            TrackProperty(property, newElementSetup);

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

            if (property.IsManagedRef())
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Clear Serialized Reference"), false, OnClearSerializedReference);
            }

            menu.ShowAsContext();
        }

        private static void TrackProperty(SerializedProperty property, Action<SerializedProperty> newElementSetup)
        {
            selectedProperty = property;
            selectedType = property.GetValue().GetType();
            selectedDisplayInfo = $"{property.displayName} ({selectedType.Name})";
            selectedElementSetup = newElementSetup;
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

                Debug.Log($"Copied {copiedDisplayInfo}");
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
            try
            {
                selectedProperty.SetValueFromJson(EditorGUIUtility.systemCopyBuffer, copiedType, $"Pasted data from clipboard to {selectedDisplayInfo}");
                CustomDrawersCache.TryResetDuplicatedElement(selectedProperty);

                Debug.Log($"Pasted {copiedDisplayInfo} to {selectedProperty.propertyPath}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void OnInsertElementAbove()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.InsertArrayElementAtIndex(index);
            parent.Apply();

            var newElement = parent.GetArrayElementAtIndex(index);
            newElement.ResetDefaultValues(selectedElementSetup, false);
        }

        private static void OnInsertElementBelow()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.InsertArrayElementAtIndex(index);
            parent.Apply();

            var newElement = parent.GetArrayElementAtIndex(index + 1);
            newElement.ResetDefaultValues(selectedElementSetup, false);
        }

        private static void OnDuplicateElement()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.InsertArrayElementAtIndex(index);
            parent.Apply();

            if (selectedProperty.IsManagedRef())
            {
                var newElement = parent.GetArrayElementAtIndex(index + 1);

                TrackProperty(selectedProperty, null);
                OnCopy();

                newElement.managedReferenceValue = Activator.CreateInstance(selectedType);
                TrackProperty(newElement, null);
                OnPaste();
            }
        }

        private static void OnDeleteElement()
        {
            selectedProperty.ExtractArrayElementInfo(out var parent, out var index);
            parent.DeleteArrayElementAtIndex(index);
            parent.Apply();
        }

        private static void OnClearSerializedReference()
        {
            selectedProperty.managedReferenceValue = null;
            selectedProperty.Apply();
        }
    }
}