using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildCharacterMorphs
{
    private const string PlayerPath = "Assets/EmberTown/Entities/Player/Logic/ET_Player_01.prefab";
    private const string Generated = "Assets/EmberTown/Generated/CharacterMorphs";
    private const string Backup = "Backup/CharacterMorphs_20261009";
    private static SkinnedMeshRenderer source;
    private static Vector3[] sourceVertices;
    private static int[] sourceTriangles;
    private static List<int>[] adjacentTriangles;
    private static readonly StringBuilder report = new StringBuilder();

    private static readonly string[,] aliases = {
        { "Eyes_Distnace", "Eyes_Distance" },
        { "Eyes_Almond_Shape", "Eyes_Almond" },
        { "Eyelashes_Eyes_Almond", "Eyes_Almond" },
        { "Eyes_Triangle_01_Shape", "Eyes Triangle_01" },
        { "Eyelashes_Eyes_Triangle_01 Shape", "Eyes Triangle_01" },
        { "Eyes_Rounded_01_Shape", "Eyes_Rounded_01" },
        { "Eyelashes_Eyes_Rounded_01 Shape", "Eyes_Rounded_01" }
    };

    public static string All()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building assets.");
        report.Clear();
        Directory.CreateDirectory(Generated);
        AssetDatabase.Refresh();
        source = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath).GetComponentsInChildren<SkinnedMeshRenderer>(true).First(r => r.name == "BaseMesh_ET");
        sourceVertices = source.sharedMesh.vertices;
        sourceTriangles = source.sharedMesh.triangles;
        adjacentTriangles = new List<int>[sourceVertices.Length];
        for (int i = 0; i < adjacentTriangles.Length; i++) adjacentTriangles[i] = new List<int>();
        for (int i = 0; i < sourceTriangles.Length; i += 3)
            for (int j = 0; j < 3; j++) adjacentTriangles[sourceTriangles[i + j]].Add(i);

        CreateEye("l", Item.WearableData.SubCategories.PupilLeft);
        CreateEye("r", Item.WearableData.SubCategories.PupilRight);

        foreach (string path in new[] {
            "Assets/EmberTown/Face/Eyebrows/001/Prefab_Eyebrows_001.prefab",
            "Assets/EmberTown/Face/Eyelashes/001/Prefab_Eyelashes_001.prefab",
            "Assets/EmberTown/Face/Eyes/Iris/001/Prefab_Eye_Iris_001.prefab",
            "Assets/EmberTown/Clothes/Base/Prefab_Base_Underwear_Bra_001.prefab",
            "Assets/EmberTown/Clothes/Base/Prefab_Base_Underwear_Boxer_001.prefab",
            "Assets/EmberTown/Clothes/Top/Blouse/001/Prefab_Top_Blouse_001.prefab",
            "Assets/EmberTown/Clothes/Bottom/Skirt/001/Prefab_Bottom_Skirt_001.prefab"
        }) PrepareItem(path);

        foreach (string path in new[] { PlayerPath, "Assets/EmberTown/Entities/Player/Logic/ET_Player.prefab" })
        {
            BackupAsset(path);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                ConfigureInventory(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        // Apply to the actual player in the currently open scene, retaining its body weights.
        foreach (InventorySystem inventory in UnityEngine.Object.FindObjectsByType<InventorySystem>(FindObjectsInactive.Include))
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(inventory);
            if (path != PlayerPath && path != "Assets/EmberTown/Entities/Player/Logic/ET_Player.prefab") continue;
            Undo.RegisterFullObjectHierarchyUndo(inventory.gameObject, "Equip Character Morph Items");
            ConfigureInventory(inventory.gameObject);
            inventory.EquipBaseItems();
            foreach (Item item in inventory.GetComponentsInChildren<Item>(true))
            {
                if (new SerializedObject(item).FindProperty("unloadOnEnable").boolValue) item.gameObject.SetActive(false);
            }
            inventory.GetComponent<BlendShapeGroups>().RefreshGroups();
            PrefabUtility.RecordPrefabInstancePropertyModifications(inventory);
            PrefabUtility.RecordPrefabInstancePropertyModifications(inventory.GetComponent<BlendShapeGroups>());
            EditorSceneManager.MarkSceneDirty(inventory.gameObject.scene);
            BackupAsset(inventory.gameObject.scene.path);
            EditorSceneManager.SaveScene(inventory.gameObject.scene);
            Selection.activeGameObject = inventory.gameObject;
            report.AppendLine("Configured and equipped scene player: " + inventory.name);
        }
        AssetDatabase.SaveAssets();
        return report.ToString();
    }

    private static void BackupAsset(string path)
    {
        string destination = Path.Combine(Backup, path);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        if (!File.Exists(destination)) File.Copy(path, destination);
        if (File.Exists(path + ".meta") && !File.Exists(destination + ".meta")) File.Copy(path + ".meta", destination + ".meta");
    }

    private static void CreateEye(string side, Item.WearableData.SubCategories category)
    {
        string modelPath = "Assets/EmberTown/Face/Eyes/Pupil/001/Eye_Pupil_001_" + side + ".fbx";
        string prefabPath = "Assets/EmberTown/Face/Eyes/Pupil/001/Prefab_Eye_Pupil_001_" + side + ".prefab";
        GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
        try
        {
            root.name = "Prefab_Eye_Pupil_001_" + side;
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            Item item = root.AddComponent<Item>();
            SerializedObject data = new SerializedObject(item);
            data.FindProperty("isWearable").boolValue = true;
            data.FindProperty("wearableData.category").intValue = (int)Item.WearableData.Categories.Face;
            data.FindProperty("wearableData.subCategory").intValue = (int)category;
            data.ApplyModifiedPropertiesWithoutUndo();
            // Retain the pupil material from the existing item.
            Material material = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberTown/Face/Eyes/Pupil/001/Prefab_Eye_Pupil_001.prefab").GetComponentInChildren<Renderer>(true).sharedMaterial;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        PrepareItem(prefabPath);
    }

    private static string Canonical(string name)
    {
        for (int i = 0; i < aliases.GetLength(0); i++) if (aliases[i, 0] == name) return aliases[i, 1];
        return name;
    }

    private static void PrepareItem(string path)
    {
        BackupAsset(path);
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            root.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one;
            bool face = root.GetComponent<Item>().Category == Item.WearableData.Categories.Face;
            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                if (!filter || !filter.sharedMesh) continue;
                Material[] materials = renderer.sharedMaterials;
                Mesh mesh = filter.sharedMesh;
                GameObject target = renderer.gameObject;
                UnityEngine.Object.DestroyImmediate(renderer);
                UnityEngine.Object.DestroyImmediate(filter);
                SkinnedMeshRenderer skin = target.AddComponent<SkinnedMeshRenderer>();
                skin.sharedMesh = mesh;
                skin.sharedMaterials = materials;
            }
            int rendererIndex = 0;
            foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!renderer.sharedMesh) continue;
                string meshPath = Generated + "/" + Path.GetFileNameWithoutExtension(path) + "_" + rendererIndex++ + ".asset";
                Mesh original = renderer.sharedMesh;
                Mesh mesh = UnityEngine.Object.Instantiate(original);
                mesh.name = Path.GetFileNameWithoutExtension(meshPath);
                Vector3[] vertices = mesh.vertices;
                SurfacePoint[] surface = new SurfacePoint[vertices.Length];
                for (int i = 0; i < vertices.Length; i++) surface[i] = ClosestSurface(vertices[i]);

                HashSet<string> existing = new HashSet<string>();
                for (int i = 0; i < mesh.blendShapeCount; i++) existing.Add(Canonical(mesh.GetBlendShapeName(i)));
                int added = 0;
                for (int shape = 0; shape < source.sharedMesh.blendShapeCount; shape++)
                {
                    string name = source.sharedMesh.GetBlendShapeName(shape);
                    if (name.StartsWith("---") || existing.Contains(Canonical(name))) continue;
                    // Clothes only need body proportions; face pieces also receive expressions.
                    if (!face && !(name.StartsWith("Body_") || name == "Legs_Shape" || name == "Arms_Shape")) continue;
                    for (int frame = 0; frame < source.sharedMesh.GetBlendShapeFrameCount(shape); frame++)
                    {
                        Vector3[] sourceDeltas = new Vector3[sourceVertices.Length];
                        Vector3[] sourceNormals = new Vector3[sourceVertices.Length];
                        Vector3[] sourceTangents = new Vector3[sourceVertices.Length];
                        source.sharedMesh.GetBlendShapeFrameVertices(shape, frame, sourceDeltas, sourceNormals, sourceTangents);
                        Vector3[] deltas = new Vector3[vertices.Length];
                        Vector3[] normals = new Vector3[vertices.Length];
                        Vector3[] tangents = new Vector3[vertices.Length];
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            SurfacePoint point = surface[i];
                            deltas[i] = point.Interpolate(sourceDeltas);
                            normals[i] = point.Interpolate(sourceNormals);
                            tangents[i] = point.Interpolate(sourceTangents);
                        }
                        mesh.AddBlendShapeFrame(name, source.sharedMesh.GetBlendShapeFrameWeight(shape, frame), deltas, normals, tangents);
                    }
                    added++;
                }

                if (renderer.bones.Length == 0 || mesh.bindposes.Length == 0)
                {
                    BoneWeight[] weights = new BoneWeight[vertices.Length];
                    BoneWeight[] bodyWeights = source.sharedMesh.boneWeights;
                    for (int i = 0; i < weights.Length; i++) weights[i] = bodyWeights[surface[i].nearest];
                    mesh.boneWeights = weights;
                    mesh.bindposes = source.sharedMesh.bindposes;
                    GameObject skeleton = new GameObject("ItemSkeleton");
                    skeleton.transform.SetParent(root.transform, false);
                    Transform[] bones = new Transform[source.bones.Length];
                    for (int i = 0; i < bones.Length; i++)
                    {
                        Transform bone = new GameObject(source.bones[i].name).transform;
                        bone.SetParent(skeleton.transform, false);
                        Matrix4x4 matrix = mesh.bindposes[i].inverse;
                        bone.localPosition = matrix.GetColumn(3);
                        bone.localRotation = matrix.rotation;
                        bone.localScale = matrix.lossyScale;
                        bones[i] = bone;
                    }
                    renderer.bones = bones;
                    renderer.rootBone = bones.FirstOrDefault(b => b.name == source.rootBone.name);
                }
                Mesh saved = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (saved) { EditorUtility.CopySerialized(mesh, saved); UnityEngine.Object.DestroyImmediate(mesh); mesh = saved; }
                else AssetDatabase.CreateAsset(mesh, meshPath);
                renderer.sharedMesh = mesh;
                renderer.localBounds = new Bounds(mesh.bounds.center, mesh.bounds.size + Vector3.one * 0.6f);
                renderer.updateWhenOffscreen = true;
                report.AppendLine(path + ": " + renderer.name + " added=" + added + " total=" + mesh.blendShapeCount + " bones=" + renderer.bones.Length);
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private struct SurfacePoint
    {
        public int a, b, c, nearest;
        public Vector3 weights;
        public Vector3 Interpolate(Vector3[] values) { return values[a] * weights.x + values[b] * weights.y + values[c] * weights.z; }
    }

    private static SurfacePoint ClosestSurface(Vector3 vertex)
    {
        int nearest = 0;
        float distance = float.MaxValue;
        for (int i = 0; i < sourceVertices.Length; i++)
        {
            float squared = (sourceVertices[i] - vertex).sqrMagnitude;
            if (squared < distance) { nearest = i; distance = squared; }
        }
        SurfacePoint result = new SurfacePoint { a = nearest, b = nearest, c = nearest, nearest = nearest, weights = new Vector3(1, 0, 0) };
        foreach (int triangle in adjacentTriangles[nearest])
        {
            int a = sourceTriangles[triangle], b = sourceTriangles[triangle + 1], c = sourceTriangles[triangle + 2];
            Vector3 weights = ClosestBarycentric(vertex, sourceVertices[a], sourceVertices[b], sourceVertices[c]);
            Vector3 point = sourceVertices[a] * weights.x + sourceVertices[b] * weights.y + sourceVertices[c] * weights.z;
            float squared = (point - vertex).sqrMagnitude;
            if (squared <= distance) { distance = squared; result = new SurfacePoint { a = a, b = b, c = c, nearest = nearest, weights = weights }; }
        }
        return result;
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
        float denominator = va + vb + vc;
        if (Mathf.Abs(denominator) < 1e-12f) return new Vector3(1, 0, 0);
        float inverse = 1f / denominator;
        float y = vb * inverse, z = vc * inverse;
        return new Vector3(1 - y - z, y, z);
    }

    private static void ConfigureInventory(GameObject root)
    {
        InventorySystem inventory = root.GetComponent<InventorySystem>();
        BlendShapeGroups groups = root.GetComponent<BlendShapeGroups>();
        if (!groups) groups = root.AddComponent<BlendShapeGroups>();
        groups.SourceMesh = root.GetComponentsInChildren<SkinnedMeshRenderer>(true).First(r => r.name == "BaseMesh_ET");
        SerializedObject groupData = new SerializedObject(groups);
        SerializedProperty aliasData = groupData.FindProperty("shapeAliases");
        aliasData.arraySize = aliases.GetLength(0);
        for (int i = 0; i < aliasData.arraySize; i++)
        {
            aliasData.GetArrayElementAtIndex(i).FindPropertyRelative("shapeName").stringValue = aliases[i, 0];
            aliasData.GetArrayElementAtIndex(i).FindPropertyRelative("groupName").stringValue = aliases[i, 1];
        }
        groupData.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject data = new SerializedObject(inventory);
        SerializedProperty slots = data.FindProperty("itemSlots");
        for (int i = slots.arraySize - 1; i >= 0; i--)
            if (slots.GetArrayElementAtIndex(i).FindPropertyRelative("socket.type").intValue == (int)Slot.Sockets.Types.Pupil) slots.DeleteArrayElementAtIndex(i);
        ConfigureSlot(slots, Slot.Sockets.Types.Iris, "Assets/EmberTown/Face/Eyes/Iris/001/Prefab_Eye_Iris_001.prefab");
        ConfigureSlot(slots, Slot.Sockets.Types.PupilLeft, "Assets/EmberTown/Face/Eyes/Pupil/001/Prefab_Eye_Pupil_001_l.prefab");
        ConfigureSlot(slots, Slot.Sockets.Types.PupilRight, "Assets/EmberTown/Face/Eyes/Pupil/001/Prefab_Eye_Pupil_001_r.prefab");
        ConfigureSlot(slots, Slot.Sockets.Types.Eyebrows, "Assets/EmberTown/Face/Eyebrows/001/Prefab_Eyebrows_001.prefab");
        ConfigureSlot(slots, Slot.Sockets.Types.Eyelashes, "Assets/EmberTown/Face/Eyelashes/001/Prefab_Eyelashes_001.prefab");
        data.ApplyModifiedPropertiesWithoutUndo();
        groups.RefreshGroups();
        EditorUtility.SetDirty(inventory);
        EditorUtility.SetDirty(groups);
    }

    private static void ConfigureSlot(SerializedProperty slots, Slot.Sockets.Types type, string path)
    {
        int index = -1;
        for (int i = 0; i < slots.arraySize; i++)
            if (slots.GetArrayElementAtIndex(i).FindPropertyRelative("socket.type").intValue == (int)type) { index = i; break; }
        if (index < 0) { index = slots.arraySize; slots.InsertArrayElementAtIndex(index); slots.GetArrayElementAtIndex(index).FindPropertyRelative("currentSlotItem").objectReferenceValue = null; }
        SerializedProperty slot = slots.GetArrayElementAtIndex(index);
        slot.FindPropertyRelative("label").stringValue = type.ToString();
        slot.FindPropertyRelative("socket.type").intValue = (int)type;
        slot.FindPropertyRelative("basePrefabItem").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Item>();
    }
}
