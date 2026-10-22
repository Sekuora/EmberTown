using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


[System.Serializable]
public class Slot
{
    // Base Properties //

    // Label
    [SerializeField]
    public string label;

    private Slot parent;

    private Sockets.Types type;


    // Inherit Type From Parent If Assigned to Constructor
    public Sockets.Types Type => parent != null ? parent.Type : type;

    // Constructor
    public Slot(string inLabel, Sockets.Types inType)
    {
        label = inLabel;
        type = inType;
    }

    // Constructor Children
    public Slot(string inLabel, Slot inParent)
    {
        label = inLabel;
        parent = inParent;

    }


    [SerializeField]
    private Sockets socket;

    // Socket Struct Public Members 
    public Sockets Socket { get => socket; private set => socket = value; }

    // Current slot item value
    [SerializeField]
    private Item currentSlotItem;

    [SerializeField]
    private Item basePrefabItem;

    // Current slot item member
    public Item CurrentSlotItem { get => currentSlotItem; set => currentSlotItem = value; }

    // Base slot item member
    public Item BasePrefabItem { get => basePrefabItem; set => basePrefabItem = value; }

    [System.Serializable]
    public struct Sockets
    {
        public enum Types
        {
            Face,
            Top,
            Bottom,
            Panties,
            Brassiere,
            Iris,
            Pupil,
            Eyebrows,
            Eyelashes,
            PupilLeft,
            PupilRight,
            IrisLeft,
            IrisRight,

        }

        public Types type;

    }

    private bool canSpawn = false;

    public bool CanSpawn { get => canSpawn; set => canSpawn = value; }


    // Subslot list.

    private readonly List<Slot> subSlots = new();

    public IReadOnlyList<Slot> SubSlots => subSlots;

    public void AddSubslot(Slot subSlot)
    {
        if (subSlot != null)
            subSlots.Add(subSlot);
    }


    // Getters / Setters
    public void SetSocketType(Sockets.Types type)
    {
        socket.type = type;
    }

    public Sockets.Types GetSocketType()
    {
        return socket.type;
    }

    public Item.WearableData.Categories GetWearableCategory(Item inItem)
    {
        return inItem.Category;
    }


}
