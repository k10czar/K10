using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public static class SerializedPropertyExtension
    {
        #region Property Content Manipulation

        public static void Apply(this SerializedProperty property, string reason = null) => PropertyCollection.Apply(property.serializedObject, reason ?? $"Modified {property.propertyPath}");

        public static void PrepareForChanges(this SerializedProperty property, string reason) => Undo.RecordObject(property.serializedObject.targetObject, reason);

        public static void ApplyDirectChanges(this SerializedProperty property)
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.Update();
            PropertyCollection.ScheduleReset(property.serializedObject);
        }

        public static object GenerateDefaultValue(this SerializedProperty property)
        {
            var type = property.GetValue().GetType();

            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type == typeof(string)) return string.Empty;
            if (type.IsArray) return Array.CreateInstance(type.GetElementType()!, 0);

            if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null)
                return Activator.CreateInstance(type);

            throw new InvalidOperationException($"Cannot create an instance of {type.FullName} because it lacks a parameterless constructor. {property.propertyPath}");
        }

        public static void ExtractArrayElementInfo(this SerializedProperty property, out SerializedProperty parentProperty, out int index)
        {
            parentProperty = null;
            index = -1;

            var path = property.propertyPath;

            var dataIndex = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
            if (dataIndex == -1) throw new ArgumentException($"Property is not an array element! {path}");

            var arrayPath = path[..dataIndex];
            parentProperty = property.serializedObject.FindProperty(arrayPath);

            var start = dataIndex + ".Array.data[".Length;
            var end = path.LastIndexOf(']');

            if (!int.TryParse(path.Substring(start, end - start), out index))
                throw new ArgumentException($"Property is not an array element! {path}");
        }

        public static void ResetDefaultValues(this SerializedProperty newElement, Action<SerializedProperty> customReset, bool keepManagedRef)
        {
            if (!keepManagedRef && newElement.IsManagedRef())
            {
                newElement.managedReferenceValue = null;
                newElement.Apply();

                EditorUtils.RunOnSceneOnce(() => SerializedRefLib.DrawTypePickerMenu(newElement, customReset));
                return;
            }

            if (customReset != null) customReset(newElement);
            else if (!CustomDrawersCache.TryResetNewElement(newElement))
            {
                if (newElement.propertyType is SerializedPropertyType.ObjectReference)
                {
                    newElement.objectReferenceValue = null;
                    newElement.Apply();
                }
                else
                {
                    newElement.PrepareForChanges("Resetting new element!");
                    newElement.SetValue(newElement.GenerateDefaultValue());
                    newElement.ApplyDirectChanges();
                }
            }
        }

        #endregion

        #region Reflection Getters / Setters

        private static readonly Regex arrayIndexRegex = new(@"\.Array\.data\[(\d+)\]$", RegexOptions.Compiled);
        private static readonly Regex extractArrayPieceRegex = new(@"\[\d+\]", RegexOptions.Compiled);

        private const BindingFlags Bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags InstanceOnlyBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static T GetParentValue<T>(this SerializedProperty property, bool canInherit = false)
        {
            object obj = property.serializedObject.targetObject;
            var fieldStructure = GetPathStructure(property);
            var targetType = typeof(T);

            if (obj.GetType() == targetType) return (T) obj;

            foreach (var pathPiece in fieldStructure)
            {
                obj = pathPiece.Contains("[")
                    ? GetFieldValueWithIndex(pathPiece, obj)
                    : GetFieldValue(pathPiece, obj);

                if (canInherit)
                {
                    if (targetType.IsAssignableFrom(obj.GetType())) return (T) obj;
                }
                else if (obj.GetType() == targetType) return (T) obj;
            }

            throw new Exception($"{targetType} was not found in property: {property.propertyPath}");
        }

        public static object GetValue(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            var fieldStructure = GetPathStructure(property);

            foreach (var pathPiece in fieldStructure)
            {
                obj = pathPiece.Contains("[")
                    ? GetFieldValueWithIndex(pathPiece, obj)
                    : GetFieldValue(pathPiece, obj);
            }

            return obj;
        }

        public static T GetValue<T>(this SerializedProperty property) where T : class => GetValue(property) as T;

        public static bool SetValue<T>(this SerializedProperty property, T value) where T : class
        {
            object obj = property.serializedObject.targetObject;
            var fieldStructure = GetPathStructure(property);

            for (var index = 0; index < fieldStructure.Length - 1; index++)
            {
                var pathPiece = fieldStructure[index];
                obj = pathPiece.Contains("[")
                    ? GetFieldValueWithIndex(pathPiece, obj)
                    : GetFieldValue(pathPiece, obj);
            }

            var lastPathPiece = fieldStructure.Last();
            return lastPathPiece.Contains("[")
                ? SetFieldValueWithIndex(lastPathPiece, obj, value)
                : SetFieldValue(lastPathPiece, obj, value);
        }

        private static FieldInfo GetField(string fieldName, object obj)
        {
            var currentType = obj.GetType();
            do
            {
                var field = currentType.GetField(fieldName, Bindings);
                if (field != null) return field;

                currentType = currentType.BaseType;
            } while (currentType != null);

            return null;
        }

        private static object GetFieldValue(string fieldName, object obj)
        {
            var field = GetField(fieldName, obj);
            return field?.GetValue(obj);
        }

        private static object GetFieldValueWithIndex(string pathPiece, object obj)
        {
            var fieldName = extractArrayPieceRegex.Replace(pathPiece, "");
            var index = GetPathDigit(pathPiece);

            var field = GetField(fieldName, obj);
            if (field == null) return null;

            var list = field.GetValue(obj);

            if (list is Array array) return array.GetValue(index);
            if (list is IList iList) return iList[index];
            return null;
        }

        private static bool SetFieldValue(string fieldName, object obj, object value)
        {
            var field = GetField(fieldName, obj);
            if (field == null) return false;

            field.SetValue(obj, value);
            return true;
        }

        private static bool SetFieldValueWithIndex(string pathPiece, object obj, object value)
        {
            var fieldName = extractArrayPieceRegex.Replace(pathPiece, "");
            var index = GetPathDigit(pathPiece);

            var field = GetField(fieldName, obj);
            if (field == null) return false;

            var list = field.GetValue(obj);

            if (list is Array array)
            {
                array.SetValue(value, index);
                return true;
            }

            if (list is IList iList)
            {
                iList[index] = value;
                return true;
            }

            return false;
        }

        private static int GetPathDigit(string pathPiece) => Convert.ToInt32(new string(pathPiece.Where(char.IsDigit).ToArray()));

        private static string[] GetPathStructure(SerializedProperty property) => property.propertyPath.Replace(".Array.data", "").Split('.');

        public static bool RemoveSelfFromArray(this SerializedProperty property)
        {
            var path = property.propertyPath;
            var match = arrayIndexRegex.Match(path);

            if (!match.Success)
            {
                Debug.LogError($"Property '{property.displayName}' is not part of an array.");
                return false;
            }

            var index = int.Parse(match.Groups[1].Value);
            var parentPath = path[..match.Index];

            var parentArray = property.serializedObject.FindProperty(parentPath);

            if (parentArray == null || !parentArray.isArray)
            {
                Debug.LogError($"Could not find parent array for '{property.displayName}' at path '{parentPath}'.");
                return false;
            }

            parentArray.DeleteArrayElementAtIndex(index);
            parentArray.Apply();

            return true;
        }


        #endregion

        #region Type Queries

        private static readonly Dictionary<string, Dictionary<string, Type>> propertyTypeCache = new();

        public static string GetCacheID(this SerializedObject serializedObject)
            => serializedObject.targetObject.GetInstanceID().ToString();

        public static Type GetCachedType(this SerializedProperty property)
        {
            var cacheID = property.serializedObject.GetCacheID();
            if (!propertyTypeCache.TryGetValue(cacheID, out var cache))
            {
                cache = new Dictionary<string, Type>();
                propertyTypeCache[cacheID] = cache;
            }

            if (cache.TryGetValue(property.propertyPath, out var cachedType))
                return cachedType;

            var value = GetValue(property);
            if (value == null) return null;

            var type = value.GetType();
            cache[property.propertyPath] = type;

            return type;
        }

        public static void InvalidateTypeCache(this SerializedObject serializedObject)
        {
            var cacheID = serializedObject.GetCacheID();
            propertyTypeCache.Remove(cacheID);
        }

        [MenuItem("Disyphus/Editor/Clear Property Type Cache")]
        public static void ClearCache() => propertyTypeCache.Clear();

        public static Type GetObjectReferenceType(this SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                Debug.LogError($"Trying to get type from non-ObjectReference property: {property.propertyPath}");
                return null;
            }

            return property.objectReferenceValue != null
                ? property.objectReferenceValue.GetType()
                : GetTypeFromPropertyTypeString(property);
        }

        private static Type GetTypeFromPropertyTypeString(SerializedProperty property)
        {
            var typeName = property.type;

            if (string.IsNullOrEmpty(typeName) || !typeName.StartsWith("PPtr<") || !typeName.EndsWith(">"))
            {
                Debug.LogError($"Don't know how to parse property type: {typeName}");
                return null;
            }

            typeName = typeName.Substring(5, typeName.Length - 6).TrimStart('$');

            foreach (var type in TypeCache.GetTypesDerivedFrom<Object>())
            {
                if (type.Name == typeName) return type;
            }

            Debug.LogError($"Couldn't find type matching property type: {typeName}");
            return null;
        }

        #endregion

        #region Utils

        public static bool IsArrayEntry(this SerializedProperty property) => arrayIndexRegex.IsMatch(property.propertyPath);
        public static string PrettyName(this SerializedProperty property) => ObjectNames.NicifyVariableName(property.name);

        public static SerializedProperty FindBackingProperty(this SerializedProperty property, string propertyName)
            => property.FindPropertyRelative($"<{propertyName}>k__BackingField");


        public static string GetReadablePath(this SerializedObject serializedObject, string propertyPath)
        {
            object current = serializedObject.targetObject;
            var currentType = current.GetType();

            var parts = propertyPath.Split('.');
            var readableParts = new List<string>();

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (part == "Array") continue;
                if (currentType == null) break;

                if (part.StartsWith("data["))
                {
                    var index = Convert.ToInt32(new string(part.Where(char.IsDigit).ToArray()));

                    if (current is IList list && index >= 0 && index < list.Count)
                    {
                        current = list[index];
                        currentType = current?.GetType();
                        readableParts.Add($"[{index}]");;
                    }
                    else
                    {
                        readableParts.Add($"[INVALID_INDEX:{index}]");
                        break;
                    }
                }
                else if (part.StartsWith("managedReferences["))
                {
                    var id = long.Parse(new string(part.Where(char.IsDigit).ToArray()));
                    var managed = ManagedReferenceUtility.GetManagedReference((Object) current, id);

                    // TODO: Get managedRef array index

                    if (managed != null)
                    {
                        current = managed;
                        currentType = managed.GetType();
                        readableParts.Add(currentType.Name);
                    }
                    else
                    {
                        readableParts.Add("[unresolved_managed_reference]");
                        break;
                    }
                }
                else
                {
                    var field = currentType.GetField(part, InstanceOnlyBindings);

                    if (field != null)
                    {
                        readableParts.Add(RemoveBackingField(field.Name));
                        current = field.GetValue(current);
                        currentType = current?.GetType();
                    }
                    else
                    {
                        readableParts.Add($"[UNKNOWN:{part}]");
                        break;
                    }
                }
            }

            return string.Join(".", readableParts);
        }

        private static string RemoveBackingField(string path) => path.Replace(".<", ".").Replace("<", "").Replace(">k__BackingField", "");

        public static bool SimpleIsValid(this SerializedProperty property)
        {
            return property != null &&
                   property.serializedObject != null &&
                   property.serializedObject.targetObject != null &&
                   property.propertyType != SerializedPropertyType.Generic;
        }

        public static bool TargetsSameProperty(this SerializedProperty a, SerializedProperty b)
        {
            return a != null && b != null &&
                   a.serializedObject.targetObject == b.serializedObject.targetObject &&
                   a.propertyPath == b.propertyPath;
        }

        #endregion

        #region Common Setters

        public static void FillWithExisting<T>(this SerializedProperty property, bool searchChildren = true, bool searchParent = true, bool force = true) where T : Component
            => FillWithExisting(property, typeof(T), force, searchChildren, searchParent);

        public static void FillWithExisting(this SerializedProperty property, Type targetType, bool searchChildren = true, bool searchParent = true, bool force = true)
        {
            if (!force && HasValidTarget(property, targetType)) return;

            var component = GetComponent(property, targetType, searchChildren, searchParent);
            if (component == null)
            {
                Debug.LogError($"Could not find {targetType} in {property.serializedObject.targetObject.name}");
                return;
            }

            property.objectReferenceValue = component;
            property.Apply();
        }

        private static Component GetComponent(SerializedProperty property, Type targetType, bool searchChildren, bool searchParent)
        {
            var root = (MonoBehaviour) property.serializedObject.targetObject;

            if (searchChildren)
            {
                var candidate = root.GetComponentInChildren(targetType, true);
                if (candidate != null) return candidate;
            }

            if (searchParent)
            {
                var candidate = root.GetComponentInParent(targetType, true);
                if (candidate != null) return candidate;
            }

            return null;
        }

        public static bool HasValidTarget<T>(SerializedProperty property) => HasValidTarget(property, typeof(T));

        private static bool HasValidTarget(SerializedProperty property, Type targetType)
        {
            return property.objectReferenceValue != null && targetType.IsAssignableFrom(property.objectReferenceValue.GetType());
        }

        #endregion

        #region JSON Manipulation

        public static void CopyValue<T>(this SerializedProperty property, T value, string reason) where T : class
        {
            var json = JsonConvert.SerializeObject(value, GetSerializationSettings());
            SetValueFromJson(property, json, typeof(T), reason);
        }

        public static string GetJson(this SerializedProperty property)
        {
            var value = property.GetValue();
            return JsonConvert.SerializeObject(value, GetSerializationSettings());
        }

        public static void SetValueFromJson(this SerializedProperty property, string json, Type valueType, string reason)
        {
            PrepareForChanges(property, reason);

            var deserializedObject = JsonConvert.DeserializeObject(json, valueType, GetSerializationSettings());
            SetValue(property, deserializedObject);

            ApplyDirectChanges(property);
        }

        private static JsonSerializerSettings GetSerializationSettings() => new()
        {
            ContractResolver = new SerializeFieldContractResolver(),
            Converters = { new UnityObjectConverter() }
        };

        #endregion
    }
}