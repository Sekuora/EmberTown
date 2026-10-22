using System.Collections.Generic;
using UnityEngine;

public class EventsSubsystem : MonoBehaviour
{

    private static EventsSubsystem instance;

    // Call Instance
    public static EventsSubsystem Call()
    {
        return instance;
    }

    [SerializeField]
    private PlayerFlagsDatabase flagsDB;

    private List<Condition> currentConditions;

    private QuestDataBridge questsData;

    private CharacterDataBridge characterData;

    private void Start()
    {
        if (!DialogueSubsystem.Call()) { Debug.Log("No Dialogue Subsystem Instance found at: " + this.name); return; }
        DialogueSubsystem.Call().OnDialogueDataEvent += DialogueEventCallback;
    }

    private void OnDisable()
    {
        DialogueSubsystem.Call().OnDialogueDataEvent -= DialogueEventCallback;
    }


    /// <summary>
    /// Entry Point for the Event Subsystem
    /// Manages dialogue events emmited by the DialogueInteraction.cs
    /// Loops through the Conditions defined in the dialogue data assets
    /// and handles them differently according to their type. 
    /// </summary>
    /// <param name="args"></param>
    private void DialogueEventCallback(DArgs args)
    {
        Conversation conversationToPlay = null;

        // Unpack Args
        List<Conversation> availableConversations = args.GetByString<List<Conversation>>("AvailableConversations");
        characterData = args.GetByString<CharacterDataBridge>("CharacterData");
        questsData = args.GetByString<QuestDataBridge>("QuestsData");

        // Guard for null conversation data
        if (availableConversations == null) { return; }

        // Get each conversation
        foreach (Conversation conversation in availableConversations)
        {

            bool canEnterConversation = true;


            currentConditions = conversation.Conditions;


            // Get each conversation conditions
            foreach (Condition condition in currentConditions)
            {


                // Manage Conversation Condition According to Type
                switch (condition.Type)
                {
                    case Condition.Types.MeetNPC:


                        bool state = flagsDB.MetNPC(characterData.CharacterName);

                        if (state != condition.CheckFlagState)
                        {
                            canEnterConversation = false;
                        }

                        break;
                }

                if (!canEnterConversation)
                {

                    break;
                }


            }

            if (canEnterConversation)
            {
                conversationToPlay = conversation;
            }


        }

        if (conversationToPlay != null)
        {
            // Emit conversation data through dialogue subsystem event
            // Subscribers: PlayerInteractionSystem
            TArgs<Conversation> conversationData = new("AvailableConversation", conversationToPlay);

            DialogueSubsystem.Call().EmitAvailableDialogueEvent(new DArgs(conversationData));
        }

    }
}


