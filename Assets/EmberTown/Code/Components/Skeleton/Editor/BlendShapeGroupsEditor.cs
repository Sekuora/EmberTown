using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlendShapeGroups))]
public class BlendShapeGroupsEditor : Editor
{
    private string shapeFilter = "";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "shapeGroups");
        BlendShapeGroups groups = (BlendShapeGroups)target;
        if (serializedObject.ApplyModifiedProperties()) groups.RefreshGroups();

        if (GUILayout.Button("Refresh Blendshape Groups"))
        {
            Undo.RecordObject(groups, "Refresh Blendshape Groups");
            groups.RefreshGroups();
            EditorUtility.SetDirty(groups);
        }

        EditorGUILayout.Space();
        shapeFilter = EditorGUILayout.TextField("Filter Shapes", shapeFilter);
        EditorGUILayout.HelpBox("Shared weights: 0 to 100. SetShape(name, 1) sets every matching mesh to 100. Source mesh animation also synchronizes in LateUpdate.", MessageType.Info);
        foreach (BlendShapeGroups.ShapeGroup group in groups.Groups)
        {
            if (!string.IsNullOrEmpty(shapeFilter) && group.shapeName.IndexOf(shapeFilter, System.StringComparison.OrdinalIgnoreCase) < 0) { continue; }
            EditorGUI.BeginChangeCheck();
            float weight = EditorGUILayout.Slider(group.shapeName, group.weight, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(groups, "Change Blendshape Group");
                foreach (SkinnedMeshRenderer mesh in groups.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    Undo.RecordObject(mesh, "Change Blendshape Group");
                groups.SetWeight(group.shapeName, weight);
                EditorUtility.SetDirty(groups);
                foreach (SkinnedMeshRenderer mesh in groups.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    EditorUtility.SetDirty(mesh);
                    if (PrefabUtility.IsPartOfPrefabInstance(mesh)) PrefabUtility.RecordPrefabInstancePropertyModifications(mesh);
                }
                if (PrefabUtility.IsPartOfPrefabInstance(groups)) PrefabUtility.RecordPrefabInstancePropertyModifications(groups);
            }
        }
    }
}
