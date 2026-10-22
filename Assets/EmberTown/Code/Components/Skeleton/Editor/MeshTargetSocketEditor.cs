using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshTargetSocket)), CanEditMultipleObjects]
public class MeshTargetSocketEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool changed = DrawDefaultInspector();
        EditorGUILayout.HelpBox("Min/Max limit automatic scale relative to the neutral head (1 = original size). Strength 0 keeps the original fit; 1 follows the head. Preserve Head Mesh Shape keeps the hairstyle; disable it for local surface deformation. Size and Offset adjust both hair pieces manually.", MessageType.Info);
        bool apply = GUILayout.Button("Apply Head Fit Settings");
        bool refresh = GUILayout.Button("Refresh Head Meshes");
        foreach (Object value in targets)
        {
            MeshTargetSocket sockets = (MeshTargetSocket)value;
            if (refresh) sockets.RefreshHeadMeshes();
            if (changed || apply)
            {
                foreach (SurfaceMeshAttachment attachment in sockets.GetComponentsInChildren<SurfaceMeshAttachment>(true)) Undo.RecordObject(attachment, "Change Head Mesh Fit");
                sockets.ApplyHeadFitSettings();
                foreach (SurfaceMeshAttachment attachment in sockets.GetComponentsInChildren<SurfaceMeshAttachment>(true)) EditorUtility.SetDirty(attachment);
            }
        }
    }
}
