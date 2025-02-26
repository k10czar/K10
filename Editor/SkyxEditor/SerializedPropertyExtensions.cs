using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class SerializedPropertyExtension
    {
        private static readonly Regex isArrayEntryRegex = new(@"\.Array\.data\[\d+\]$", RegexOptions.Compiled);
        private static readonly Regex replaceRegex = new(@"\[\d+\]", RegexOptions.Compiled);
        private const BindingFlags Bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static object GetValue(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');

            for (int i = 0; i < fieldStructure.Length; i++)
            {
                var pathPiece = fieldStructure[i];
                if (pathPiece.Contains("["))
                {
                    int index = System.Convert.ToInt32(new string(pathPiece.Where(char.IsDigit).ToArray()));
                    obj = GetFieldValueWithIndex(replaceRegex.Replace(pathPiece, ""), obj, index);
                }
                else obj = GetFieldValue(pathPiece, obj);
            }

            return obj;
        }

        public static T GetValue<T>(this SerializedProperty property) where T : class
            => GetValue(property) as T;

        public static bool SetValue<T>(this SerializedProperty property, T value) where T : class
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            for (int i = 0; i < fieldStructure.Length - 1; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(char.IsDigit).ToArray()));
                    obj = GetFieldValueWithIndex(replaceRegex.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }

            string fieldName = fieldStructure.Last();
            if (fieldName.Contains("["))
            {
                int index = System.Convert.ToInt32(new string(fieldName.Where(char.IsDigit).ToArray()));
                return SetFieldValueWithIndex(replaceRegex.Replace(fieldName, ""), obj, index, value);
            }
            else
            {
                return SetFieldValue(fieldName, obj, value);
            }
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
            return field != null ? field.GetValue(obj) : default;
        }

        private static object GetFieldValueWithIndex(string fieldName, object obj, int index)
        {
            var field = GetField(fieldName, obj);
            if (field == null) return default;

            var list = field.GetValue(obj);
            if (list.GetType().IsArray) return ((object[])list)[index];

            return list is IEnumerable ? ((IList)list)[index] : default;
        }

        private static bool SetFieldValue(string fieldName, object obj, object value)
        {
            var field = GetField(fieldName, obj);
            if (field == null) return false;

            field.SetValue(obj, value);
            return true;
        }

        private static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value)
        {
            var field = GetField(fieldName, obj);
            if (field == null) return false;

            object list = field.GetValue(obj);
            if (list.GetType().IsArray)
            {
                ((object[])list)[index] = value;
                return true;
            }
            else if (list is IEnumerable)
            {
                ((IList)list)[index] = value;
                return true;
            }

            return false;
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

        public static bool IsArrayEntry(this SerializedProperty property) => isArrayEntryRegex.IsMatch(property.propertyPath);
        public static string PrettyName(this SerializedProperty property) => ObjectNames.NicifyVariableName(property.name);

        public static void Apply(this SerializedProperty property, string reason = null) => PropertyCollection.Apply(property.serializedObject, reason ?? $"Modified {property.propertyPath}");
    }
}