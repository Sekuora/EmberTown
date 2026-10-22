using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Main Inventory System for Player and other game entitiese that store items.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    // Components
    private EntityDataBridge playerData;

    private RetargetMeshSkeleton retargetMeshSkeleton;

    [SerializeField]
    private List<Slot> itemSlots;

    private Slot top;

    // Properties
    [SerializeField]
    private Item TestItem;


    private void Awake()
    {

        playerData = GetComponent<EntityDataBridge>();
        retargetMeshSkeleton = GetComponent<RetargetMeshSkeleton>();

        if (!playerData)
        {
            Debug.Log("No Player Data Bridge found on " + gameObject.name);
            return;
        }

        if (!retargetMeshSkeleton)
        {
            Debug.Log("No Retarget Mesh Skeleton found on " + gameObject.name);
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        foreach (Slot slot in itemSlots)
        {
            Equip(slot.BasePrefabItem);
        }
    }


    /// <summary>
    /// The equip method combines the Validation and Spawning of Items.
    /// </summary>
    /// <param name="inItem"></param>
    /// Item to equip.
    private void Equip(Item inItem)
    {
        if (!inItem) { return; }
        if (inItem.IsWearable)
        {
            if (ValidateItemCategory(inItem, out Slot targetSlot))
            {
                SpawnSlotItem(targetSlot, inItem);
                // Debug.Log(targetSlot.GetSocketType().ToString() + " : " + targetSlot.label);
            }
        }
    }

    /// <summary>
    /// Validation for socket available items. 
    /// If the item category doesn't match the target slot it won't equip.
    /// </summary>
    /// <param name="inItem"></param>
    /// The item to validate-
    /// <param name="targetSlot"></param>
    /// The target gathered from the validation.
    /// <returns></returns>
    private bool ValidateItemCategory(Item inItem, out Slot targetSlot)
    {
        targetSlot = null;


        foreach (Slot slot in itemSlots)
        {
            if (inItem.Category == Item.WearableData.Categories.Underwear)
            {

                if (slot.Socket.type == Slot.Sockets.Types.Brassiere && inItem.SubCategory == Item.WearableData.SubCategories.Brassiere)
                {
                    targetSlot = slot;

                    return true;
                }

                if (slot.Socket.type == Slot.Sockets.Types.Panties && inItem.SubCategory == Item.WearableData.SubCategories.Panties)
                {
                    targetSlot = slot;

                    return true;
                }

            }



            if (inItem.Category == Item.WearableData.Categories.Top)
            {
                if (slot.Socket.type == Slot.Sockets.Types.Top)
                {
                    targetSlot = slot;

                    return true;
                }

            }

            if (inItem.Category == Item.WearableData.Categories.Bottom)
            {
                if (slot.Socket.type == Slot.Sockets.Types.Bottom)
                {
                    targetSlot = slot;

                    return true;
                }

            }


            if (inItem.Category == Item.WearableData.Categories.Face)
            {
                if (slot.Socket.type == Slot.Sockets.Types.Iris && inItem.SubCategory == Item.WearableData.SubCategories.Iris)
                {
                    targetSlot = slot;

                    return true;
                }

                if (slot.Socket.type == Slot.Sockets.Types.Pupil && inItem.SubCategory == Item.WearableData.SubCategories.Pupil)
                {
                    targetSlot = slot;

                    return true;
                }

                if (slot.Socket.type == Slot.Sockets.Types.Eyebrows && inItem.SubCategory == Item.WearableData.SubCategories.Eyebrows)
                {
                    targetSlot = slot;

                    return true;
                }

                if (slot.Socket.type == Slot.Sockets.Types.Eyelashes && inItem.SubCategory == Item.WearableData.SubCategories.Eyelashes)
                {
                    targetSlot = slot;

                    return true;
                }



                if (slot.Socket.type == Slot.Sockets.Types.Face && inItem.SubCategory == Item.WearableData.SubCategories.Default)
                {
                    targetSlot = slot;

                    return true;
                }

            }




        }

        return false;

    }

    /// <summary>
    /// Destroy the prefab game object source and spawn an instance to use for retargetting.
    /// This method is used within the Equip method.
    /// </summary>
    /// <param name="inSlot"></param>
    /// The slot to equip the item.
    /// <param name="inItem"></param>
    /// The item to equip into the slot.
    private void SpawnSlotItem(Slot inSlot, Item inItem)
    {
        if (inSlot.CurrentSlotItem)
        {
            Destroy(inSlot.CurrentSlotItem.gameObject);
        }

        Item worldItem = Instantiate(inItem, transform);

        inSlot.CurrentSlotItem = worldItem;

        retargetMeshSkeleton.Retarget(worldItem);
    }

}
