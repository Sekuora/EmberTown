using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Emits dialogue subsystem events. 
/// </summary>



public class DialogueInteraction : MonoBehaviour
{

    // Components
    private DialoguePlayer dsPlayer;


    private void Start()
    {
        dsPlayer = DialogueSubsystem.Call().DSPlayer;
    }


    private void OnTriggerStay(Collider other)
    {
        // Return if conversation is already active
        if (dsPlayer.State == DialoguePlayer.States.Active) { return; }

        // Get other object components, return if they aren't present
        if (!other.TryGetComponent(out DialogueDataBridge dialogueDataBridge) ||
            !other.TryGetComponent(out CharacterDataBridge characterDataBridge) ||
            !other.TryGetComponent(out QuestDataBridge questDataBridge))
        {
            return;
        }

        // Prepare Arguments to emit. Managed in the event subsystem

        // Conversations list from dialogue data
        TArgs<List<Conversation>> conversationsData = new TArgs<List<Conversation>>("AvailableConversations", dialogueDataBridge.Conversations);

        // Character Data
        TArgs<CharacterDataBridge> characterData = new TArgs<CharacterDataBridge>("CharacterData", characterDataBridge);

        // Quest Data
        TArgs<QuestDataBridge> questData = new TArgs<QuestDataBridge>("QuestsData", questDataBridge);

        // Check conversations data exists
        if (conversationsData == null)
        {
            return;
        }

        // Check if DialoguePlayer is in range
        dsPlayer.State = DialoguePlayer.States.InRange;


        // Emit arguments with Dialogue Data Event
        DialogueSubsystem.Call().EmitDialogueDataEvent(new DArgs(conversationsData, characterData, questData));

    }

    private void OnTriggerExit(Collider other)
    {
        // Set state to interrupted
        dsPlayer.State = DialoguePlayer.States.Ended;

        // Assign new state args
        var stateArgs = new TArgs<DialoguePlayer.States>("State", dsPlayer.State);

        // Emit event to dialogue UI Listeners
        DialogueSubsystem.Call().EmitDialogueDataEvent(new DArgs(stateArgs));
    }
}
