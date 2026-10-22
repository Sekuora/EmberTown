using System;
using System.Linq;
using UnityEngine;

public class EntityDataBridge : MonoBehaviour
{
    /// <summary>
    ///  Player Properties
    /// </summary>
    [Header("Definition")]
    // States Data
    [SerializeField]
    private EntityTypes entityTypes;
    [Space(10)]

    [Header("Personalization")]
    [Header("Hairstyles")]
    [Space(10)]
    [SerializeField]
    private GameObject[] hairStylesFront;

    [SerializeField]
    private GameObject[] hairStylesBack;

    [Header("Clothes")]
    [Space(10)]
    [SerializeField]
    private Item[] clothesBase;
    [SerializeField]
    private Item[] clothesTop;
    [SerializeField]
    private Item[] clothesBottom;

    [SerializeField]
    private Item[] faceWearables = Array.Empty<Item>();
    private Item[] availableClothes;

    [Header("Movement Settings")]
    [SerializeField]
    private float walkSpeed = 2f;
    [SerializeField]
    private float walkAcceleration = 0.25f;
    [SerializeField]
    private float walkDeceleration = 0.25f;
    [SerializeField]
    private float rotationSpeed = 10f;

    [SerializeField]
    float accelerationFactor;

    [SerializeField]
    private float runSpeed = 4f;
    [SerializeField]
    private float runAcceleration = 0.25f;

    [SerializeField]
    private float wallCollisionDistance = 0.6f;



    [SerializeField]
    private float gravityMultiplier = 1f;

    private float gravity = -9.81f;



    // Public Members
    public EntityTypes EntityType { get => entityTypes; set => entityTypes = value; }

    // Get / Set Members
    public GameObject[] HairStylesFront { get => hairStylesFront; private set => hairStylesFront = value; }

    public GameObject[] HairStylesBack { get => hairStylesBack; private set => hairStylesBack = value; }

    public Item[] ClothesBase { get => clothesBase; private set => clothesBase = value; }

    public Item[] ClothesTop { get => clothesTop; private set => clothesTop = value; }

    public Item[] ClothesBottom { get => clothesBottom; private set => clothesBottom = value; }
    public float RunSpeed { get => runSpeed; set => runSpeed = value; }
    public float RunAcceleration { get => runAcceleration; set => runAcceleration = value; }
    public float WalkAcceleration { get => walkAcceleration; set => walkAcceleration = value; }
    public float WalkSpeed { get => walkSpeed; set => walkSpeed = value; }
    public float WalkDeceleration { get => walkDeceleration; set => walkDeceleration = value; }
    public float WallCollisionDistance { get => wallCollisionDistance; set => wallCollisionDistance = value; }
    public float GravityMultiplier { get => gravityMultiplier; set => gravityMultiplier = value; }
    public float Gravity { get => gravity; set => gravity = value; }
    public float AccelerationFactor { get => accelerationFactor; set => accelerationFactor = value; }
    public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }




    // State Enums
    public enum EntityTypes
    {
        Player,
        NPC,
        Enemy

    }

    public Item[] GetAvailableClothes()
    {
        availableClothes = new Item[clothesBase.Length + clothesTop.Length + clothesBottom.Length + faceWearables.Length];

        Array.Copy(clothesBase, 0, availableClothes, 0, clothesBase.Length);

        Array.Copy(clothesTop, 0, availableClothes, clothesBase.Length, clothesTop.Length);

        Array.Copy(clothesBottom, 0, availableClothes, clothesBase.Length + clothesTop.Length, clothesBottom.Length);

        Array.Copy(faceWearables, 0, availableClothes, clothesBase.Length + clothesTop.Length + clothesBottom.Length, faceWearables.Length);


        return availableClothes;


    }



}
