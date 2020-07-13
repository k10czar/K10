using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (BaseHashedSOCollection), true)]
public class BaseHashedSOCollectionEditor : Editor {
	private string _title;

	void OnEnable () {
		_title = target.NameOrNull ();
		for (int i = _title.Length - 1; i > 0; i--) {
			var c = _title[i];
			if (!char.IsUpper (c)) continue;
			_title = _title.Insert (i, " ");
		}

	}
	public override void OnInspectorGUI () {
		var collection = (BaseHashedSOCollection) target;
		int size = collection.Count;

		var count = 0;
		for (int i = 0; i < size; i++) {
			var entry = collection.GetElementBase (i);
			if (entry == null) continue;
			count++;
		}

		SeparationLine.Horizontal ();
		EditorGUILayout.LabelField ($"{_title} ({count})", K10GuiStyles.bigBoldCenterStyle, GUILayout.Height (28));
		SeparationLine.Horizontal ();

		var edit = (IHashedSOCollectionEditor) collection;

		EditorGUILayout.BeginVertical ();
		var lastValid = -1;
		for (int i = 0; i < size; i++) {
			var entry = collection.GetElementBase (i) as IHashedSO;
			if (entry == null) {
				if (lastValid + 1 == i) GUILayout.Space (5);
				continue;
			}
			lastValid = i;
			EditorGUILayout.BeginHorizontal ();
			var hasConflict = (entry.HashID < 0 || entry.HashID != i);
			if (hasConflict) GuiColorManager.New (Color.red);
			if (IconButton.Layout ("objective", 's')) Selection.SetActiveObjectWithContext (entry as Object, entry as Object);
			EditorGUILayout.LabelField ("[" + i.ToString () + "]", GUILayout.Width (30f));

			var tryResolve = hasConflict && GUILayout.Button ("!!CONFLICT!!");

			EditorGUI.BeginDisabledGroup (true);
			EditorGUILayout.ObjectField (entry as Object, collection.GetElementType (), false);
			EditorGUI.EndDisabledGroup ();

			if (hasConflict) GuiColorManager.Revert ();

			EditorGUILayout.EndHorizontal ();

			if (tryResolve) edit.TryResolveConflict (i);
		}
		EditorGUILayout.EndVertical ();
		if (GUILayout.Button ("Check Consistency")) edit.EditorCheckConsistency ();
		if (edit.EditorCanChangeIDsToOptimizeSpace && GUILayout.Button ("Optimize")) edit.EditorTryOptimize ();

		GuiColorManager.New (Color.red);
		if (GUILayout.Button ("!DANGER! Enforce HashIDs")) edit.Editor_HACK_EnforceHashIDs();
		GuiColorManager.Revert ();
	}
}