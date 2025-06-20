using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class EditorSerializationLib
    {
        #region GetPropertyFromArray

        public static SerializedProperty GetBooleansProp(SerializedProperty root, int index = 0, bool defaultValue = false)
        {
            var prop = GetPropertyFromArray(root, "booleans", out var createdFields, index);

            if (createdFields)
            {
                prop.boolValue = defaultValue;
                prop.Apply();
            }

            return prop;
        }

        public static SerializedProperty GetNumbersProp(SerializedProperty root, int index = 0, int defaultValue = 0)
        {
            var prop = GetPropertyFromArray(root, "numbers", out var createdFields, index);

            if (createdFields)
            {
                prop.intValue = defaultValue;
                prop.Apply();
            }

            return prop;
        }

        public static SerializedProperty GetStringsProp(SerializedProperty root, int index = 0, string defaultValue = "", string fieldName = "strings")
        {
            var prop = GetPropertyFromArray(root, fieldName, out var createdFields, index);

            if (createdFields)
            {
                prop.stringValue = defaultValue;
                prop.Apply();
            }

            return prop;
        }

        public static SerializedProperty GetPositionsProp(SerializedProperty root, int index = 0)
        {
            var prop = GetPropertyFromArray(root, "positions", out var createdFields, index);
            if (createdFields) prop.Apply();
            return prop;
        }

        public static SerializedProperty GetDirectionsProp(SerializedProperty root, int index = 0)
        {
            var prop = GetPropertyFromArray(root, "directions", out var createdFields, index);
            if (createdFields) prop.Apply();
            return prop;
        }

        public static SerializedProperty GetObjectsProp(SerializedProperty root, int index = 0)
        {
            var prop = GetPropertyFromArray(root, "objects", out var createdFields, index);
            if (createdFields) prop.Apply();
            return prop;
        }

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
            arrayProp.Apply();
        }

        public static void AddPropertyToArray(SerializedProperty root, string name)
        {
            var arrayProp = root.FindPropertyRelative(name);
            arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
            arrayProp.Apply();
        }

        #endregion
    }
}