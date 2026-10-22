using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


/// <summary>
/// System to make a skinned mesh follow a matching main skeleton. 
/// Omit bones that don't match the main skeleton.
/// </summary>
public class RetargetMeshSkeleton : MonoBehaviour
{

    // Properties
    // private Item[] meshes;

    [Header("References")]


    [SerializeField]
    private Transform rootBone;


    // Members
    // public Item[] Meshes { get => meshes; set => meshes = value; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //Retarget();
    }



    // Retarget bones from similar skeletons to main skeleton
    [ContextMenu("Retarget Functionality")]
    public void Retarget(Item inMesh)
    {
        if (!rootBone) { return; }



        // Bone hierarchy dictionary
        Dictionary<string, Transform> boneDictionary = new Dictionary<string, Transform>();

        // Get assigned root bone
        Transform[] baseBones = rootBone.GetComponentsInChildren<Transform>();




        // Build original sk dictionary
        foreach (Transform bone in baseBones)
        {
            if (!boneDictionary.ContainsKey(bone.name))
            {
                boneDictionary.Add(bone.name, bone);
            }
        }


        if (!inMesh) return;

        SkinnedMeshRenderer targetSkinMesh = inMesh.GetComponentInChildren<SkinnedMeshRenderer>();

        // Debug.Log("Found " + targetSkinMesh + " skinned meshes in " + inMesh.name);


        // Iterate skin meshes and retarget bones to the base skeleton


        // Transfer bones from target mesh to base skeleton
        Transform[] bonesToTransfer = targetSkinMesh.bones;
        Transform[] bonesRetarget = new Transform[bonesToTransfer.Length];

        // Remap bones with matching names
        for (int j = 0; j < bonesToTransfer.Length; j++)
        {

            Transform sourceBone = bonesToTransfer[j];

            if (sourceBone && boneDictionary.TryGetValue(sourceBone.name, out Transform matchingBone))
            {
                bonesRetarget[j] = matchingBone;
            }
            else
            {
                bonesRetarget[j] = sourceBone;
            }

        }

        targetSkinMesh.bones = bonesRetarget;

        // Assign retargeted bones to the target mesh
        if (targetSkinMesh.rootBone && boneDictionary.TryGetValue(targetSkinMesh.rootBone.name, out Transform matchingRoot))
        {
            targetSkinMesh.rootBone = matchingRoot;
        }
    }



    // Retarget bones from similar skeletons to main skeleton
    public void RetargetBatch(Item[] meshes)
    {
        if (!rootBone) { return; }


        // Bone hierarchy dictionary
        Dictionary<string, Transform> boneDictionary = new Dictionary<string, Transform>();

        // Get assigned root bone
        Transform[] baseBones = rootBone.GetComponentsInChildren<Transform>();




        // Build original sk dictionary
        foreach (Transform bone in baseBones)
        {
            if (!boneDictionary.ContainsKey(bone.name))
            {
                boneDictionary.Add(bone.name, bone);
            }
        }

        foreach (Item mesh in meshes)
        {
            if (!mesh) continue;

            SkinnedMeshRenderer[] targetSkinMeshes = mesh.GetComponentsInChildren<SkinnedMeshRenderer>();


            // Iterate skin meshes and retarget bones to the base skeleton
            foreach (SkinnedMeshRenderer targetSkinMesh in targetSkinMeshes)
            {

                // Transfer bones from target mesh to base skeleton
                Transform[] bonesToTransfer = targetSkinMesh.bones;
                Transform[] bonesRetarget = new Transform[bonesToTransfer.Length];

                // Remap bones with matching names
                for (int j = 0; j < bonesToTransfer.Length; j++)
                {

                    Transform sourceBone = bonesToTransfer[j];

                    if (sourceBone && boneDictionary.TryGetValue(sourceBone.name, out Transform matchingBone))
                    {
                        bonesRetarget[j] = matchingBone;
                    }
                    else
                    {
                        bonesRetarget[j] = sourceBone;
                    }

                }

                targetSkinMesh.bones = bonesRetarget;

                // Assign retargeted bones to the target mesh
                if (targetSkinMesh.rootBone && boneDictionary.TryGetValue(targetSkinMesh.rootBone.name, out Transform matchingRoot))
                {
                    targetSkinMesh.rootBone = matchingRoot;
                }
            }
        }
    }
}
