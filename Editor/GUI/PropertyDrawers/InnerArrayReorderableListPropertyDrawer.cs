using UnityEditor;

public abstract class InnerArrayReorderableListPropertyDrawer : ReorderableListPropertyDrawer
{
    protected abstract string ArrayPropertyName { get; }
    protected override SerializedProperty GetArrayProperty( SerializedProperty prop ) { return prop.FindPropertyRelative( ArrayPropertyName ); }
}