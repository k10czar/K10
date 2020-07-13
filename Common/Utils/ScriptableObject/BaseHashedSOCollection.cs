using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public interface IHashedSOCollectionEditor {
	void EditorTryOptimize ();
	void EditorCheckConsistency ();
	bool TryResolveConflict (int i);
	bool EditorRequestMember (Object t, bool forceCorrectPosition = false);
	bool EditorCanChangeIDsToOptimizeSpace { get; }
	void Editor_HACK_Remove (int id);
	void Editor_HACK_EnforceHashIDs();
}
#endif

public interface IHashedSOCollection
#if UNITY_EDITOR
	: IHashedSOCollectionEditor
#endif
{
	bool ContainsHashID (int hashID);
	bool Contains (IHashedSO obj);
	int Count { get; }
	IHashedSO GetElementBase (int hashID);
}

public abstract class BaseHashedSOCollection : ScriptableObject, IHashedSOCollection, System.Collections.IEnumerable {
	public abstract int Count { get; }
	public abstract IHashedSO GetElementBase (int index);
	public abstract bool Contains (IHashedSO element);
	public abstract bool ContainsHashID (int hashID);

	IEnumerator IEnumerable.GetEnumerator () {
		var count = Count;
		for (int i = 0; i < count; i++) {
			var element = GetElementBase (i);
			if (element != null) yield return element;
		}
	}

	public abstract System.Type GetElementType ();

#if UNITY_EDITOR
	public abstract void Editor_HACK_Remove (int id);

	void IHashedSOCollectionEditor.Editor_HACK_EnforceHashIDs() {
		HashSet<IHashedSO> _alreadyVisited = new HashSet<IHashedSO> ();

		for (int i = 0; i < Count; i++) {
			var element = GetElementBase (i);
			if (element == null) continue;
			if (element.HashID != i) continue;

			_alreadyVisited.Add (element);
		}

		for (int j = 0; j < Count; j++) {
			var element = GetElementBase (j);
			if (element == null) continue;
			
			if (_alreadyVisited.Contains (element)) {
				
				if (element.HashID != j) 
				{
					Editor_HACK_Remove (j);
					j--;
				}

				continue;
			}
			_alreadyVisited.Add (element);
			if (element.HashID == j) continue;
			element.SetHashID (j);
			EditorUtility.SetDirty (element as ScriptableObject);
		}
	}

	void IHashedSOCollectionEditor.EditorCheckConsistency () {
		var guids = AssetDatabase.FindAssets ($"t:{GetElementType().ToString()}");
		for (int i = 0; i < guids.Length; i++) {
			var path = AssetDatabase.GUIDToAssetPath (guids[i]);
			var asset = AssetDatabase.LoadAssetAtPath (path, GetElementType ());
			((IHashedSOCollectionEditor) this).EditorRequestMember (asset, true);
		}
	}

	bool IHashedSOCollectionEditor.EditorRequestMember (Object obj, bool forceCorrectPosition = false) {
		var t = obj as IHashedSO;
		if (t == null) return false;

		if (Contains (t)) {
			var element = GetElementBase (t.HashID);
			if (element != t || forceCorrectPosition) SetRealPosition (t);
			return false;
		}

		int hashID = t.HashID;
		if (hashID < 0 || hashID >= Count || GetElementBase (hashID) != t) {
			var assetPath = AssetDatabase.GetAssetPath ((Object) t);
			var assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID (assetPath);
			bool isDuplicateFromOtherFile = t.GUID != assetGuid;

			if (!isDuplicateFromOtherFile) {
				if (!ResolveConflictedFile (t, assetPath)) return false;
			}
		}

		AddElement (t);
		((IHashedSOEditor) t).SetHashID (Count - 1);

		EditorUtility.SetDirty ((Object) t);
		EditorUtility.SetDirty (this);

		return true;
	}

	void IHashedSOCollectionEditor.EditorTryOptimize () {
		if (!((IHashedSOCollectionEditor) this).EditorCanChangeIDsToOptimizeSpace) return;

		Clear ();
		((IHashedSOCollectionEditor) this).EditorCheckConsistency ();
	}

	public abstract bool EditorCanChangeIDsToOptimizeSpace { get; }
	protected abstract void Clear ();
	protected abstract bool AddElement (IHashedSO obj);
	protected abstract bool ResolveConflictedFile (IHashedSO t, string assetPath);
	public abstract bool TryResolveConflict (int i);
	protected abstract bool SetRealPosition (IHashedSO obj);
#endif
}