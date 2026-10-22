using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Base class for data asset to hold each character quests data. 
/// </summary>
public abstract class QuestDataAsset : MonoBehaviour
{
    // Private
    private List<Quest> quests = new List<Quest>();

    // Public
    public List<Quest> Quests { get => quests; set => quests = value; }

    protected void Awake()
    {

        InitializeQuestData();
    }

    protected void AddQuest(Quest inQuest)
    {
        quests.Add(inQuest);
    }

    protected void InitializeQuestData()
    {

        // Get all methods in this specific class (public and private)
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Loop through every method it found
        foreach (MethodInfo method in methods)
        {
            // Filter: Does it return a Conversation? Does it have 0 parameters? Does the name start with "Root"?
            if (method.ReturnType == typeof(Quest) &&
                method.GetParameters().Length == 0 &&
                method.Name.StartsWith("Quest"))
            {
                // Execute the method to get the Conversation object
                Quest generatedQuest = (Quest)method.Invoke(this, null);

                // Add it to the list
                if (generatedQuest != null)
                {
                    AddQuest(generatedQuest);
                }
            }
        }

    }









}