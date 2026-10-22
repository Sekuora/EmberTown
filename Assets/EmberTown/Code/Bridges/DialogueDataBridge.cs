using System.Collections.Generic;
using UnityEngine;


public class DialogueDataBridge : MonoBehaviour
{


    [SerializeField]
    private DialogueDataAsset dialogueDataAsset;

    private List<Conversation> conversations = new List<Conversation>();


    private void Start()
    {
        conversations = dialogueDataAsset.DialogueConversations;
    }



    public void AddConversation(Conversation inConversation)
    {
        conversations.Add(inConversation);
    }


    public void AddConversations(List<Conversation> inConversations)
    {
        foreach (Conversation conversation in inConversations)
        {
            conversations.Add(conversation);
        }
    }

    public Conversation GetDialogueById(string inIDLabel)
    {

        foreach (Conversation conversation in conversations)
        {
            if (inIDLabel == conversation.IDLabel)
            {
                return conversation;
            }
        }

        return default;

    }


    public List<Conversation> Conversations { get => conversations; set => conversations = value; }



}