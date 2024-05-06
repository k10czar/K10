using UnityEditor;
using UnityEngine;

public static class CollectionElementSoftReferenceEditorExtensions
{
	public static void OnGUICollectionElementSoftRef<T>( this SerializedProperty elementRef, Rect area, IHashedSOCollection collection ) where T : Object, IHashedSO
	{
        var id = elementRef.FindPropertyRelative("_id");

		T element = null;
		try { element = (T)collection.GetElementBase( id.intValue ); }
		catch( System.Exception ex ) { }

		var newElement = EditorGUI.ObjectField( area, element, typeof(T), false ) as T;
		if( newElement != element ) 
		{
			if( newElement == null ) elementRef.ClearCollectionSoftRef();
            else elementRef.SetCollectionSoftRef( newElement );
        }
	}

    public static void SetCollectionSoftRef<T>( this SerializedProperty elementRef, T newLoot ) where T : Object, IHashedSO
    {
		elementRef.FindPropertyRelative("_id").intValue = newLoot.HashID;
        elementRef.FindPropertyRelative("_editorAssetRefGuid").stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newLoot));
        elementRef.FindPropertyRelative("_assetHardReference").objectReferenceValue = newLoot;
    }

    public static void ClearCollectionSoftRef( this SerializedProperty elementRef )
    {
		elementRef.FindPropertyRelative("_id").intValue = -1;
        elementRef.FindPropertyRelative("_editorAssetRefGuid").stringValue = null;
        elementRef.FindPropertyRelative("_assetHardReference").objectReferenceValue = null;
    }
}
