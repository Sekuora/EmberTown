using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shared blendshape weights for the body and its equipped items.
/// </summary>
[ExecuteAlways]
public class BlendShapeGroups : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private SkinnedMeshRenderer sourceMesh;

    [SerializeField]
    private bool followSourceMesh = true;

    [SerializeField]
    private List<ShapeAlias> shapeAliases = new List<ShapeAlias>();

    [SerializeField, HideInInspector]
    private List<ShapeGroup> shapeGroups = new List<ShapeGroup>();

    private readonly Dictionary<string, List<ShapeTarget>> shapeTargets = new Dictionary<string, List<ShapeTarget>>();
    private readonly Dictionary<string, int> sourceShapes = new Dictionary<string, int>();
    private readonly HashSet<SkinnedMeshRenderer> registeredMeshes = new HashSet<SkinnedMeshRenderer>();

    public SkinnedMeshRenderer SourceMesh { get => sourceMesh; set => sourceMesh = value; }
    public IReadOnlyList<ShapeGroup> Groups => shapeGroups;

    [System.Serializable]
    public class ShapeGroup
    {
        public string shapeName;
        [Range(0f, 100f)]
        public float weight;
    }

    [System.Serializable]
    public struct ShapeAlias
    {
        public string shapeName;
        public string groupName;
    }

    private struct ShapeTarget
    {
        public SkinnedMeshRenderer mesh;
        public int index;
    }

    private void OnEnable()
    {
        RefreshGroups();
    }

    private void LateUpdate()
    {
        Synchronize();
    }

    public string GetGroupName(string shapeName)
    {
        foreach (ShapeAlias alias in shapeAliases)
        {
            if (alias.shapeName == shapeName && !string.IsNullOrEmpty(alias.groupName))
            {
                return alias.groupName;
            }
        }
        return shapeName;
    }

    [ContextMenu("Refresh Blendshape Groups")]
    public void RefreshGroups()
    {
        shapeTargets.Clear();
        sourceShapes.Clear();
        registeredMeshes.Clear();

        if (!sourceMesh)
        {
            foreach (SkinnedMeshRenderer mesh in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (mesh.sharedMesh && !mesh.GetComponentInParent<Item>() && mesh.sharedMesh.blendShapeCount > 0)
                {
                    sourceMesh = mesh;
                    break;
                }
            }
        }

        if (sourceMesh && sourceMesh.sharedMesh)
        {
            for (int i = 0; i < sourceMesh.sharedMesh.blendShapeCount; i++)
            {
                string shapeName = GetGroupName(sourceMesh.sharedMesh.GetBlendShapeName(i));
                if (!shapeName.StartsWith("---")) sourceShapes[shapeName] = i;
            }
            RegisterMesh(sourceMesh);
        }

        foreach (SkinnedMeshRenderer mesh in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            BlendShapeGroups owner = mesh.GetComponentInParent<BlendShapeGroups>();
            if (owner == this) RegisterMesh(mesh);
        }

        shapeGroups.RemoveAll(group => !shapeTargets.ContainsKey(group.shapeName));
        Synchronize();
    }

    public void RegisterItem(Item inItem)
    {
        if (!inItem) { return; }
        foreach (SkinnedMeshRenderer mesh in inItem.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            RegisterMesh(mesh);
        }
        Synchronize();
    }

    public void UnregisterItem(Item inItem)
    {
        if (!inItem) { return; }
        foreach (SkinnedMeshRenderer mesh in inItem.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            registeredMeshes.Remove(mesh);
            foreach (List<ShapeTarget> targets in shapeTargets.Values)
            {
                targets.RemoveAll(target => target.mesh == mesh);
            }
        }
    }

    private void RegisterMesh(SkinnedMeshRenderer mesh)
    {
        if (!mesh || !mesh.sharedMesh || !registeredMeshes.Add(mesh)) { return; }

        for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
        {
            string shapeName = GetGroupName(mesh.sharedMesh.GetBlendShapeName(i));
            if (shapeName.StartsWith("---")) { continue; }
            if (!shapeTargets.TryGetValue(shapeName, out List<ShapeTarget> targets))
            {
                targets = new List<ShapeTarget>();
                shapeTargets.Add(shapeName, targets);
                ShapeGroup group = shapeGroups.Find(value => value.shapeName == shapeName);
                if (group == null)
                {
                    group = new ShapeGroup { shapeName = shapeName, weight = mesh.GetBlendShapeWeight(i) };
                    shapeGroups.Add(group);
                }
            }
            targets.Add(new ShapeTarget { mesh = mesh, index = i });
        }
    }

    /// <summary>
    /// Normalized value: 1 matches a Unity blendshape weight of 100.
    /// </summary>
    public bool SetShape(string shapeName, float value)
    {
        return SetWeight(shapeName, value * 100f);
    }

    public bool SetWeight(string shapeName, float weight)
    {
        if (string.IsNullOrEmpty(shapeName) || float.IsNaN(weight) || float.IsInfinity(weight)) { return false; }
        string groupName = GetGroupName(shapeName);
        ShapeGroup group = shapeGroups.Find(value => value.shapeName == groupName);
        if (group == null) { return false; }
        group.weight = weight;
        ApplyGroup(group);
        return true;
    }

    public void Synchronize()
    {
        foreach (ShapeGroup group in shapeGroups)
        {
            if (followSourceMesh && sourceMesh && sourceMesh.sharedMesh && sourceShapes.TryGetValue(group.shapeName, out int index))
            {
                group.weight = sourceMesh.GetBlendShapeWeight(index);
            }
            ApplyGroup(group);
        }
    }

    private void ApplyGroup(ShapeGroup group)
    {
        if (!shapeTargets.TryGetValue(group.shapeName, out List<ShapeTarget> targets)) { return; }
        foreach (ShapeTarget target in targets)
        {
            if (!target.mesh || !target.mesh.sharedMesh || target.index >= target.mesh.sharedMesh.blendShapeCount) { continue; }
            if (!Mathf.Approximately(target.mesh.GetBlendShapeWeight(target.index), group.weight))
            {
                target.mesh.SetBlendShapeWeight(target.index, group.weight);
            }
        }
    }
}
