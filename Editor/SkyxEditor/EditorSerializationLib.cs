using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class EditorSerializationLib
    {
        #region GetPropertyFromArray

        public static SerializedProperty GetBooleansProp(SerializedProperty root, int index = 0, bool defaultValue = false)
        {
            var prop = GetPropertyFromArray(root, "booleans", out var createdFields, index);
            if (createdFields) prop.boolValue = defaultValue;
            return prop;
        }

        public static SerializedProperty GetNumbersProp(SerializedProperty root, int index = 0, int defaultValue = 0)
        {
            var prop = GetPropertyFromArray(root, "numbers", out var createdFields, index);
            if (createdFields) prop.intValue = defaultValue;
            return prop;
        }

        public static SerializedProperty GetStringsProp(SerializedProperty root, int index = 0, string defaultValue = "", string fieldName = "strings")
        {
            var prop = GetPropertyFromArray(root, fieldName, out var createdFields, index);
            if (createdFields) prop.stringValue = defaultValue;
            return prop;
        }

        public static SerializedProperty GetFloatsProp(SerializedProperty root, int index = 0, float defaultValue = 0)
        {
            var prop = GetPropertyFromArray(root, "floats", out var createdFields, index);
            if (createdFields) prop.floatValue = defaultValue;
            return prop;
        }

        public static SerializedProperty GetPositionsProp(SerializedProperty root, int index = 0) => GetPropertyFromArray(root, "positions", out _, index);
        public static SerializedProperty GetDirectionsProp(SerializedProperty root, int index = 0) => GetPropertyFromArray(root, "directions", out _, index);
        public static SerializedProperty GetObjectsProp(SerializedProperty root, int index = 0) => GetPropertyFromArray(root, "objects", out _, index);
        public static SerializedProperty GetColorProp(SerializedProperty root, int index = 0) => GetPropertyFromArray(root, "colors", out _, index);

        private static SerializedProperty GetPropertyFromArray(SerializedProperty root, string name, out bool createdFields, int index = 0)
        {
            var arrayProp = root.FindPropertyRelative(name);

            createdFields = index > arrayProp.arraySize - 1;

            for (int i = arrayProp.arraySize; i < index + 1; i++)
                arrayProp.InsertArrayElementAtIndex(i);

            return arrayProp.GetArrayElementAtIndex(index);
        }

        public static int GetPropertyArraySize(SerializedProperty root, string name)
            => root.FindPropertyRelative(name).arraySize;

        public static void RemovePropertyFromArray(SerializedProperty root, string name, int index = 0)
        {
            var arrayProp = root.FindPropertyRelative(name);
            if (arrayProp.arraySize < index + 1) return;

            arrayProp.DeleteArrayElementAtIndex(index);
        }

        public static void AddPropertyToArray(SerializedProperty root, string name)
        {
            var arrayProp = root.FindPropertyRelative(name);
            arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
        }

        #endregion
    }
}