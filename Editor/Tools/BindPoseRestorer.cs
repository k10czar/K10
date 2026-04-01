using UnityEngine;
using UnityEditor;

public static class BindPoseRestorer
{
    [MenuItem("K10/Mesh/Restore Bind Pose (T-Pose) %#t")]
    static void RestoreBindPose()
    {
        var selected = Selection.activeGameObject;
        if (selected == null) return;

        var smr = selected.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found.");
            return;
        }

        var mesh = smr.sharedMesh;
        var bones = smr.bones;
        var bindposes = mesh.bindposes;

        Undo.RecordObjects(bones, "Restore Bind Pose");

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null) continue;

            // bindpose is the inverse of the bone's world matrix in T-pose
            Matrix4x4 worldMatrix = smr.transform.localToWorldMatrix * bindposes[i].inverse;

            bones[i].position = worldMatrix.GetPosition();
            bones[i].rotation = worldMatrix.rotation;
            bones[i].localScale = Vector3.one;
        }

        Debug.Log("Bind pose restored successfully.");
    }
}