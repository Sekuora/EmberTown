using UnityEngine;
using System.Collections.Generic;
using System.Reflection;


public abstract class DialogueDataAsset : MonoBehaviour
{

    // Private
    private List<Conversation> dialogueConversations = new List<Conversation>();

    // Public
    public List<Conversation> DialogueConversations { get => dialogueConversations; }

    protected void Awake()
    {

        InitializeConversationData();
    }

    protected void AddConversation(Conversation inConversation)
    {
        dialogueConversations.Add(inConversation);
    }

    protected void InitializeConversationData()

    {
        // Get all methods in this specific class (public and private)
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Loop through every method it found
        foreach (MethodInfo method in methods)
        {
            // Filter: Does it return a Conversation? Does it have 0 parameters? Does the name start with "Root"?
            if (method.ReturnType == typeof(Conversation) &&
                method.GetParameters().Length == 0 &&
                method.Name.StartsWith("Root"))
            {
                // Execute the method to get the Conversation object
                Conversation generatedConversation = (Conversation)method.Invoke(this, null);

                // Add it to the list
                if (generatedConversation != null)
                {
                    AddConversation(generatedConversation);
                }
            }
        }
    }

}