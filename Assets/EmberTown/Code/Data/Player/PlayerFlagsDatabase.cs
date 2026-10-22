using System.Collections.Generic;
using UnityEngine;


public class PlayerFlagsDatabase : MonoBehaviour
{

    private static PlayerFlagsDatabase instance;
    public static PlayerFlagsDatabase Call()
    {
        return instance;
    }


    // KnownNPCS

    private Dictionary<CharacterSubsystem.Names, bool> knownNPCs = new Dictionary<CharacterSubsystem.Names, bool>();


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }


        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFlags();

    }

    public bool MetNPC(CharacterSubsystem.Names npcToKnow)
    {
        if (knownNPCs.TryGetValue(npcToKnow, out bool metNPC))
        {

            return metNPC;

        }

        return false;

    }


    public void SetKnowNPC(CharacterSubsystem.Names npcEnum, bool state)
    {

        knownNPCs[npcEnum] = state;

    }

    private void InitializeFlags()
    {
        // System.Enum.GetValues grabs every single entry in your CharacterNames enum
        foreach (CharacterSubsystem.Names npc in System.Enum.GetValues(typeof(CharacterSubsystem.Names)))
        {
            // If it's a new enum value, add it with 'false'
            if (!knownNPCs.ContainsKey(npc))
            {
                knownNPCs.Add(npc, false);
            }
        }
    }

}