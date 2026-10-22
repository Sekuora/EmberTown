using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fits an attached mesh to the deformed surface influenced by its target bone.
/// Uses geometry rather than blendshape names or fixed scale multipliers.
/// </summary>
[ExecuteAlways]
[DefaultExecutionOrder(200)]
public class SurfaceMeshAttachment : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private SkinnedMeshRenderer sourceMesh;

    [SerializeField]
    private Transform targetBone;

    [SerializeField, Range(0f, 1f)]
    private float minimumBoneWeight = 0.95f;

    [Header("Fit Range")]
    [SerializeField, Min(0.01f)]
    private float minimumScale = 0.5f;

    [SerializeField, Min(0.01f)]
    private float maximumScale = 2f;

    [SerializeField, Range(0f, 2f)]
    private float fitStrength = 1f;

    [SerializeField, Tooltip("Follow the deformed head volume while preserving the hairstyle. Disable for local surface deformation.")]
    private bool preserveMeshShape = true;

    [Header("Manual Adjustment")]
    [SerializeField]
    private Vector3 sizeMultiplier = Vector3.one;

    [SerializeField, Tooltip("Offset in meters along the head bone's axes.")]
    private Vector3 positionOffset;

    [SerializeField, Range(0f, 1f), Tooltip("Minimum outward separation as a fraction of the attached surface triangle size.")]
    private float surfaceSeparation = 0.1f;

    private Mesh sourceAsset;
    private Transform boundBone;
    private Mesh bakedSource;
    private Matrix4x4 referenceToBone;
    private Vector3[] referenceVertices;
    private int[] referenceTriangles;
    private List<int>[] adjacentTriangles;
    private readonly List<int> surfaceVertices = new List<int>();
    private readonly List<Vector3> currentVertices = new List<Vector3>();
    [SerializeField, HideInInspector]
    private List<MeshBinding> meshBindings = new List<MeshBinding>();
    private bool isInitialized;
    private bool bindingFailed;
    private Bounds referenceBounds;

    public SkinnedMeshRenderer SourceMesh => sourceMesh;
    public Transform TargetBone => targetBone;

    [Serializable]
    private class MeshBinding
    {
        public MeshFilter filter;
        public SkinnedMeshRenderer skin;
        public Transform transform;
        public Mesh originalMesh;
        [NonSerialized] public Mesh mesh;
        public Transform[] originalBones;
        public Transform originalRoot;
        public Bounds originalBounds;
        [NonSerialized] public SurfacePoint[] points;
        [NonSerialized] public SurfacePoint[] outsidePoints;
        [NonSerialized] public Vector3[] vertices;
        [NonSerialized] public Vector3[] restVertices;
    }

    private struct SurfacePoint
    {
        public int a, b, c;
        public Vector3 weights;
        public Vector3 offset;
        public Matrix4x4 restFrame;
    }

    public void Initialize(SkinnedMeshRenderer inSource, Transform inBone)
    {
        if (sourceMesh == inSource && targetBone == inBone && isInitialized) { return; }
        ReleaseMeshes();
        sourceMesh = inSource;
        targetBone = inBone;
    }

    public void SetFitSettings(float inMinimum, float inMaximum, float inStrength, Vector3 inSize, Vector3 inOffset, bool inPreserveShape = true)
    {
        minimumScale = Mathf.Max(0.01f, inMinimum);
        maximumScale = Mathf.Max(minimumScale, inMaximum);
        fitStrength = inStrength;
        sizeMultiplier = inSize;
        positionOffset = inOffset;
        preserveMeshShape = inPreserveShape;
    }

    private void OnEnable()
    {
        // Persistent references restore the original meshes after scene/domain reloads.
        ReleaseMeshes();
    }

    private void LateUpdate()
    {
        FitSurface();
    }

    [ContextMenu("Rebind Surface")]
    public void Rebind()
    {
        ReleaseMeshes();
        FitSurface();
    }

    public void FitSurface()
    {
        if (!sourceMesh || !sourceMesh.sharedMesh || !targetBone) { return; }
        if (sourceAsset != sourceMesh.sharedMesh || boundBone != targetBone) ReleaseMeshes();
        if (bindingFailed) { return; }
        if (!isInitialized && !BindSurface()) { bindingFailed = true; return; }

        sourceMesh.BakeMesh(bakedSource);
        bakedSource.GetVertices(currentVertices);
        if (currentVertices.Count != referenceVertices.Length)
        {
            ReleaseMeshes();
            return;
        }

        Matrix4x4 sourceToBone = targetBone.worldToLocalMatrix * sourceMesh.transform.localToWorldMatrix;
        for (int i = 0; i < currentVertices.Count; i++) currentVertices[i] = sourceToBone.MultiplyPoint3x4(currentVertices[i]);

        Bounds currentBounds = new Bounds(currentVertices[surfaceVertices[0]], Vector3.zero);
        foreach (int index in surfaceVertices) currentBounds.Encapsulate(currentVertices[index]);
        Vector3 limits = new Vector3(ScaleLimit(currentBounds.size.x, referenceBounds.size.x), ScaleLimit(currentBounds.size.y, referenceBounds.size.y), ScaleLimit(currentBounds.size.z, referenceBounds.size.z));
        Vector3 volumeScale = new Vector3(VolumeScale(currentBounds.size.x, referenceBounds.size.x), VolumeScale(currentBounds.size.y, referenceBounds.size.y), VolumeScale(currentBounds.size.z, referenceBounds.size.z));
        Vector3 offsetWorld = targetBone.rotation * positionOffset;

        foreach (MeshBinding binding in meshBindings)
        {
            if (!binding.transform || !binding.mesh) { continue; }
            Matrix4x4 boneToMesh = binding.transform.worldToLocalMatrix * targetBone.localToWorldMatrix;
            for (int i = 0; i < binding.points.Length; i++)
            {
                Vector3 position;
                if (preserveMeshShape)
                {
                    position = currentBounds.center + Vector3.Scale(binding.restVertices[i] - referenceBounds.center, volumeScale);
                    // Keep the existing silhouette, but push embedded vertices outside the skull.
                    SurfacePoint point = binding.outsidePoints[i];
                    Vector3 a = currentVertices[point.a];
                    Vector3 b = currentVertices[point.b];
                    Vector3 c = currentVertices[point.c];
                    Vector3 surface = a * point.weights.x + b * point.weights.y + c * point.weights.z;
                    Vector3 normal = Vector3.Cross(b - a, c - a);
                    float area = normal.magnitude;
                    if (area > 1e-20f)
                    {
                        normal /= area;
                        Vector3 restSurface = referenceVertices[point.a] * point.weights.x + referenceVertices[point.b] * point.weights.y + referenceVertices[point.c] * point.weights.z;
                        Vector3 restNormal = Vector3.Cross(referenceVertices[point.b] - referenceVertices[point.a], referenceVertices[point.c] - referenceVertices[point.a]);
                        if (Vector3.Dot(restNormal, restSurface - referenceBounds.center) < 0f) normal = -normal;
                        float separation = Mathf.Sqrt(area) * surfaceSeparation;
                        float distance = Vector3.Dot(position - surface, normal);
                        if (distance < separation) position += normal * (separation - distance);
                    }
                }
                else
                {
                    SurfacePoint point = binding.points[i];
                    Vector3 a = currentVertices[point.a];
                    Vector3 b = currentVertices[point.b];
                    Vector3 c = currentVertices[point.c];
                    Matrix4x4 frame = SurfaceFrame(a, b, c);
                    if (Mathf.Abs(frame.determinant) < 1e-15f) frame = point.restFrame;
                    Vector3 separation = point.offset;
                    separation.z = Mathf.Max(separation.z, surfaceSeparation);
                    position = a * point.weights.x + b * point.weights.y + c * point.weights.z + frame.MultiplyVector(separation);
                    position = currentBounds.center + Vector3.Scale(position - currentBounds.center, limits);
                }
                position = Vector3.LerpUnclamped(binding.restVertices[i], position, fitStrength);
                position = currentBounds.center + Vector3.Scale(position - currentBounds.center, sizeMultiplier);
                binding.vertices[i] = boneToMesh.MultiplyPoint3x4(position) + binding.transform.worldToLocalMatrix.MultiplyVector(offsetWorld);
            }
            binding.mesh.vertices = binding.vertices;
            binding.mesh.RecalculateNormals();
            binding.mesh.RecalculateTangents();
            binding.mesh.RecalculateBounds();
            if (binding.skin) binding.skin.localBounds = binding.mesh.bounds;
        }
    }

    private float ScaleLimit(float currentSize, float originalSize)
    {
        if (originalSize < 1e-10f || currentSize < 1e-10f) return 1f;
        float scale = currentSize / originalSize;
        return Mathf.Clamp(scale, Mathf.Max(0.01f, minimumScale), Mathf.Max(minimumScale, maximumScale)) / scale;
    }

    private float VolumeScale(float currentSize, float originalSize)
    {
        if (originalSize < 1e-10f) return 1f;
        return Mathf.Clamp(currentSize / originalSize, Mathf.Max(0.01f, minimumScale), Mathf.Max(minimumScale, maximumScale));
    }

    private bool BindSurface()
    {
        sourceAsset = sourceMesh.sharedMesh;
        boundBone = targetBone;
        if (!sourceAsset.isReadable)
        {
            Debug.LogWarning("Surface attachment needs Read/Write enabled on " + sourceAsset.name, this);
            return false;
        }
        int boneIndex = Array.IndexOf(sourceMesh.bones, targetBone);
        if (boneIndex < 0 || boneIndex >= sourceAsset.bindposes.Length) { return false; }

        referenceToBone = sourceAsset.bindposes[boneIndex];
        referenceVertices = sourceAsset.vertices;
        for (int i = 0; i < referenceVertices.Length; i++) referenceVertices[i] = referenceToBone.MultiplyPoint3x4(referenceVertices[i]);
        referenceTriangles = sourceAsset.triangles;
        BoneWeight[] weights = sourceAsset.boneWeights;
        if (weights.Length != referenceVertices.Length) { return false; }
        Transform[] bones = sourceMesh.bones;
        bool[] headBones = new bool[bones.Length];
        for (int i = 0; i < bones.Length; i++) headBones[i] = bones[i] && (bones[i] == targetBone || bones[i].IsChildOf(targetBone));
        bool[] included = new bool[referenceVertices.Length];
        surfaceVertices.Clear();
        for (int i = 0; i < weights.Length; i++)
        {
            BoneWeight weight = weights[i];
            float influence = (headBones[weight.boneIndex0] ? weight.weight0 : 0f) + (headBones[weight.boneIndex1] ? weight.weight1 : 0f) + (headBones[weight.boneIndex2] ? weight.weight2 : 0f) + (headBones[weight.boneIndex3] ? weight.weight3 : 0f);
            included[i] = influence >= minimumBoneWeight;
        }
        adjacentTriangles = new List<int>[referenceVertices.Length];
        for (int i = 0; i < adjacentTriangles.Length; i++) adjacentTriangles[i] = new List<int>();
        for (int i = 0; i < referenceTriangles.Length; i += 3)
        {
            int a = referenceTriangles[i], b = referenceTriangles[i + 1], c = referenceTriangles[i + 2];
            if (!included[a] || !included[b] || !included[c]) { continue; }
            if (Mathf.Abs(SurfaceFrame(referenceVertices[a], referenceVertices[b], referenceVertices[c]).determinant) < 1e-15f) { continue; }
            adjacentTriangles[a].Add(i);
            adjacentTriangles[b].Add(i);
            adjacentTriangles[c].Add(i);
        }
        for (int i = 0; i < adjacentTriangles.Length; i++) if (adjacentTriangles[i].Count > 0) surfaceVertices.Add(i);
        if (surfaceVertices.Count == 0) { return false; }
        referenceBounds = new Bounds(referenceVertices[surfaceVertices[0]], Vector3.zero);
        foreach (int index in surfaceVertices) referenceBounds.Encapsulate(referenceVertices[index]);

        foreach (MeshFilter filter in GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.GetComponentInParent<SurfaceMeshAttachment>() != this || !filter.sharedMesh) { continue; }
            BindMesh(new MeshBinding { filter = filter, transform = filter.transform, originalMesh = filter.sharedMesh });
        }
        foreach (SkinnedMeshRenderer skin in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (skin == sourceMesh || skin.GetComponentInParent<SurfaceMeshAttachment>() != this || !skin.sharedMesh) { continue; }
            BindMesh(new MeshBinding { skin = skin, transform = skin.transform, originalMesh = skin.sharedMesh, originalBones = skin.bones, originalRoot = skin.rootBone, originalBounds = skin.localBounds });
        }
        bakedSource = new Mesh { name = "Surface Attachment Source" };
        isInitialized = meshBindings.Count > 0;
        return isInitialized;
    }

    private void BindMesh(MeshBinding binding)
    {
        if (!binding.originalMesh.isReadable)
        {
            Debug.LogWarning("Surface attachment needs Read/Write enabled on " + binding.originalMesh.name, this);
            return;
        }
        binding.vertices = binding.originalMesh.vertices;
        binding.points = new SurfacePoint[binding.vertices.Length];
        binding.outsidePoints = new SurfacePoint[binding.vertices.Length];
        binding.restVertices = new Vector3[binding.vertices.Length];
        Matrix4x4 meshToBone = targetBone.worldToLocalMatrix * binding.transform.localToWorldMatrix;
        for (int i = 0; i < binding.vertices.Length; i++)
        {
            binding.restVertices[i] = meshToBone.MultiplyPoint3x4(binding.vertices[i]);
            binding.points[i] = ClosestSurface(binding.restVertices[i]);
            binding.outsidePoints[i] = RadialSurface(binding.restVertices[i], binding.points[i]);
        }

        binding.mesh = Instantiate(binding.originalMesh);
        binding.mesh.name = binding.originalMesh.name + " Surface Instance";
        binding.mesh.hideFlags = HideFlags.DontSave;
        binding.mesh.ClearBlendShapes();
        binding.mesh.boneWeights = Array.Empty<BoneWeight>();
        binding.mesh.bindposes = Array.Empty<Matrix4x4>();
        binding.mesh.MarkDynamic();
        meshBindings.Add(binding);
        if (binding.filter) binding.filter.sharedMesh = binding.mesh;
        if (binding.skin)
        {
            binding.skin.bones = Array.Empty<Transform>();
            binding.skin.rootBone = null;
            binding.skin.sharedMesh = binding.mesh;
        }
    }

    private SurfacePoint ClosestSurface(Vector3 vertex)
    {
        int nearest = surfaceVertices[0];
        float distance = float.MaxValue;
        foreach (int index in surfaceVertices)
        {
            float squared = (referenceVertices[index] - vertex).sqrMagnitude;
            if (squared < distance) { distance = squared; nearest = index; }
        }
        SurfacePoint result = new SurfacePoint();
        distance = float.MaxValue;
        foreach (int triangle in adjacentTriangles[nearest])
        {
            int a = referenceTriangles[triangle], b = referenceTriangles[triangle + 1], c = referenceTriangles[triangle + 2];
            Vector3 weights = ClosestBarycentric(vertex, referenceVertices[a], referenceVertices[b], referenceVertices[c]);
            Vector3 point = referenceVertices[a] * weights.x + referenceVertices[b] * weights.y + referenceVertices[c] * weights.z;
            float squared = (point - vertex).sqrMagnitude;
            if (squared >= distance) { continue; }
            distance = squared;
            Matrix4x4 frame = SurfaceFrame(referenceVertices[a], referenceVertices[b], referenceVertices[c]);
            result = new SurfacePoint { a = a, b = b, c = c, weights = weights, offset = frame.inverse.MultiplyVector(vertex - point), restFrame = frame };
        }
        return result;
    }

    private SurfacePoint RadialSurface(Vector3 vertex, SurfacePoint fallback)
    {
        Vector3 direction = vertex - referenceBounds.center;
        float farthest = 0f;
        SurfacePoint result = fallback;
        for (int i = 0; i < referenceTriangles.Length; i += 3)
        {
            int a = referenceTriangles[i], b = referenceTriangles[i + 1], c = referenceTriangles[i + 2];
            if (adjacentTriangles[a].Count == 0 || adjacentTriangles[b].Count == 0 || adjacentTriangles[c].Count == 0) continue;
            Vector3 normal = Vector3.Cross(referenceVertices[b] - referenceVertices[a], referenceVertices[c] - referenceVertices[a]);
            float denominator = Vector3.Dot(normal, direction);
            if (Mathf.Abs(denominator) < 1e-20f) continue;
            float distance = Vector3.Dot(normal, referenceVertices[a] - referenceBounds.center) / denominator;
            if (distance <= farthest) continue;
            Vector3 point = referenceBounds.center + direction * distance;
            Vector3 weights = ClosestBarycentric(point, referenceVertices[a], referenceVertices[b], referenceVertices[c]);
            Vector3 closest = referenceVertices[a] * weights.x + referenceVertices[b] * weights.y + referenceVertices[c] * weights.z;
            if ((closest - point).sqrMagnitude > normal.magnitude * 1e-6f) continue;
            farthest = distance;
            result = new SurfacePoint { a = a, b = b, c = c, weights = weights };
        }
        return result;
    }

    private static Matrix4x4 SurfaceFrame(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 first = b - a;
        Vector3 second = c - a;
        Vector3 normal = Vector3.Cross(first, second);
        float length = normal.magnitude;
        Matrix4x4 frame = Matrix4x4.identity;
        frame.SetColumn(0, new Vector4(first.x, first.y, first.z, 0f));
        frame.SetColumn(1, new Vector4(second.x, second.y, second.z, 0f));
        Vector3 third = length > 1e-20f ? normal / Mathf.Sqrt(length) : Vector3.zero;
        frame.SetColumn(2, new Vector4(third.x, third.y, third.z, 0f));
        return frame;
    }

    private static Vector3 ClosestBarycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a, ac = c - a, ap = p - a;
        float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0) return new Vector3(1, 0, 0);
        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3) return new Vector3(0, 1, 0);
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0 && d1 >= 0 && d3 <= 0) { float v = d1 / (d1 - d3); return new Vector3(1 - v, v, 0); }
        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6) return new Vector3(0, 0, 1);
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0 && d2 >= 0 && d6 <= 0) { float w = d2 / (d2 - d6); return new Vector3(1 - w, 0, w); }
        float va = d3 * d6 - d5 * d4;
        if (va <= 0 && d4 - d3 >= 0 && d5 - d6 >= 0) { float w = (d4 - d3) / ((d4 - d3) + (d5 - d6)); return new Vector3(0, 1 - w, w); }
        float inverse = 1f / (va + vb + vc);
        float y = vb * inverse, z = vc * inverse;
        return new Vector3(1 - y - z, y, z);
    }

    private void OnDisable()
    {
        ReleaseMeshes();
    }

    private void OnDestroy()
    {
        ReleaseMeshes();
    }

    private void ReleaseMeshes()
    {
        foreach (MeshBinding binding in meshBindings)
        {
            if (binding.filter) binding.filter.sharedMesh = binding.originalMesh;
            if (binding.skin)
            {
                binding.skin.sharedMesh = binding.originalMesh;
                binding.skin.bones = binding.originalBones;
                binding.skin.rootBone = binding.originalRoot;
                binding.skin.localBounds = binding.originalBounds;
            }
            DestroyMesh(binding.mesh);
        }
        meshBindings.Clear();
        DestroyMesh(bakedSource);
        bakedSource = null;
        sourceAsset = null;
        boundBone = null;
        isInitialized = false;
        bindingFailed = false;
    }

    private void DestroyMesh(Mesh mesh)
    {
        if (!mesh) { return; }
        if (Application.isPlaying) Destroy(mesh);
        else DestroyImmediate(mesh);
    }
}
