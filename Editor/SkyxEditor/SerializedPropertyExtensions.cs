using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class SerializedPropertyExtension
    {
        static readonly Regex isArrayEntryRegex = new(@"\.Array\.data\[\d+\]$", RegexOptions.Compiled);
        static readonly Regex replaceRegex = new(@"\[\d+\]", RegexOptions.Compiled);

        public static T GetValue<T>(this SerializedProperty property) where T : class
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

            return (T)obj;
        }

        public static bool SetValue<T>(this SerializedProperty property, T value) where T : class
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            for (int i = 0; i < fieldStructure.Length - 1; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
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
                int index = System.Convert.ToInt32(new string(fieldName.Where(c => char.IsDigit(c)).ToArray()));
                return SetFieldValueWithIndex(replaceRegex.Replace(fieldName, ""), obj, index, value);
            }
            else
            {
                return SetFieldValue(fieldName, obj, value);
            }
        }

        private static object GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
        {
            var field = obj.GetType().GetField(fieldName, bindings);
            return field != null ? field.GetValue(obj) : default;
        }

        private static object GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var field = obj.GetType().GetField(fieldName, bindings);
            if (field == null) return default;

            var list = field.GetValue(obj);
            if (list.GetType().IsArray) return ((object[])list)[index];

            return list is IEnumerable ? ((IList)list)[index] : default;
        }

        public static bool SetFieldValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field == null) return false;

            field.SetValue(obj, value);
            return true;
        }

        public static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
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

        public static bool IsArrayEntry(this SerializedProperty property) => isArrayEntryRegex.IsMatch(property.propertyPath);

        public static string PrettyName(this SerializedProperty property) => ObjectNames.NicifyVariableName(property.name);
    }
}