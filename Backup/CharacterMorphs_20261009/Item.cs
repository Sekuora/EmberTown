using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Item : MonoBehaviour
{

    [SerializeField]
    private bool unloadOnEnable;

    [SerializeField]
    private bool isWearable = false;


    public bool IsWearable { get => isWearable; private set => isWearable = value; }

    // Category Data Access
    private WearableData.Categories category;

    private WearableData.SubCategories subCategory;

    public WearableData.Categories Category => wearableData.category;

    public WearableData.SubCategories SubCategory => wearableData.subCategory;

    // Wearable Properties Struct
    [SerializeField]
    private WearableData wearableData;


    // Function Member
    public WearableData GetWearableData()
    {
        return wearableData;
    }



    // Structs Definition
    public struct BaseProperties
    {
        public string Label { get; private set; }
        public float Price { get; private set; }

    }




    [System.Serializable]
    public struct WearableData
    {
        public enum Categories
        {
            Face,
            Top,
            Bottom,
            Underwear,
        }

        public enum SubCategories
        {
            Default,
            Panties,
            Brassiere,
            Iris,
            Pupil,
            Eyebrows,
            Eyelashes
        }

        public Categories category;

        public SubCategories subCategory;

    }

    private void OnEnable()
    {
        if (unloadOnEnable)
        {
            gameObject.SetActive(false);
        }
    }

}
