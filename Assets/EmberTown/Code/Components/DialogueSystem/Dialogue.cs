using System.Diagnostics;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

/// <summary>
/// Create the dialogue
/// Fluent patter to create a dialogue object.
/// Used to create the actual conversation data
/// </summary>
public class Dialogue
{

    private Conversation conversation;
    private CharacterSubsystem.Names currentSpeaker = CharacterSubsystem.Names.Unknown;



    // Initializer without Conditions

    public Dialogue(string inIDLabel)
    {
        conversation = new Conversation
        {
            IDLabel = inIDLabel

        };
    }


    public Dialogue(string inIDLabel, Action inCallbackEvent = null)
    {
        conversation = new Conversation
        {
            IDLabel = inIDLabel,

            CallbackEvent = inCallbackEvent

        };


    }

    // Initializer with Conditions
    public Dialogue(string inIDLabel, List<Condition> inConditions)
    {
        conversation = new Conversation
        {
            IDLabel = inIDLabel,
            Conditions = inConditions
        };
    }

    public Dialogue(string inIDLabel, List<Condition> inConditions, Action inCallbackEvent = null)
    {
        conversation = new Conversation
        {
            IDLabel = inIDLabel,
            Conditions = inConditions,
            CallbackEvent = inCallbackEvent
        };
    }



    public Dialogue Speaker(CharacterSubsystem.Names inSpeaker)
    {
        currentSpeaker = inSpeaker;
        return this;
    }


    // Line without Conditions
    public Dialogue Line(string inText)
    {
        conversation.Lines.Add
        (
            new Line
            {
                Speaker = currentSpeaker,
                Text = inText,

            }
        );

        return this;
    }

    // Line without Conditions and Callback
    public Dialogue Line(string inText, Action inCallbackEvent = null)
    {
        conversation.Lines.Add
        (
            new Line
            {
                Speaker = currentSpeaker,
                Text = inText,
                CallbackEvent = inCallbackEvent

            }
        );

        return this;
    }

    // Line with Conditions
    public Dialogue Line(string inText, List<Condition> inConditions)
    {
        conversation.Lines.Add
        (
            new Line
            {
                Speaker = currentSpeaker,
                Text = inText,
                Conditions = inConditions,

            }
        );

        return this;
    }

    // Line with Conditions and Callback
    public Dialogue Line(string inText, List<Condition> inConditions, Action inCallbackEvent = null)
    {
        conversation.Lines.Add
        (
            new Line
            {
                Speaker = currentSpeaker,
                Text = inText,
                Conditions = inConditions,
                CallbackEvent = inCallbackEvent

            }
        );

        return this;
    }


    // Choice with Conversation object reference
    public Dialogue Choice(string inLabel, Conversation inConversation)
    {
        if (conversation.Lines.Count == 0) { return this; }

        // Match current line from total conversation lines count
        Line currentLine = conversation.Lines[conversation.Lines.Count - 1];

        // Add to current line index
        currentLine.Choices.Add
        (
            new Choice
            {
                Label = inLabel,
                Conversation = inConversation,
            }

        );

        return this;
    }


    // Choice
    public Dialogue Choice(string inLabel, Func<Conversation> inConversation)
    {
        if (conversation.Lines.Count == 0) { return this; }

        Line currentLine = conversation.Lines[conversation.Lines.Count - 1];

        currentLine.Choices.Add
        (
            new Choice
            {
                Label = inLabel,
                Conversation = inConversation(),
            }
        );

        return this;
    }

    // Choice with Callback Event
    public Dialogue Choice(string inLabel, Func<Conversation> inConversation, Action inCallbackEvent = null)
    {
        if (conversation.Lines.Count == 0) { return this; }

        Line currentLine = conversation.Lines[conversation.Lines.Count - 1];

        currentLine.Choices.Add
        (
            new Choice
            {
                Label = inLabel,
                Conversation = inConversation(),
                CallbackEvent = inCallbackEvent
            }
        );

        return this;
    }


    /// <summary>
    /// Specific Quest method to insert a quest below the current Line
    /// </summary>
    /// <param name="inQuestID"></param>
    /// <returns></returns>
    public Dialogue Quest_Line(string inQuestID)
    {
        if (conversation.Lines.Count == 0) { return this; }

        Line currentLine = conversation.Lines[conversation.Lines.Count - 1];

        currentLine.QuestID = inQuestID;

        return this;

    }

    /// <summary>
    /// Specific Quest method to insert a quest below the current Line.Choice
    /// </summary>
    /// <param name="inQuestID"></param>
    /// <returns></returns>
    public Dialogue Quest_Choice(string inQuestID)
    {
        if (conversation.Lines.Count == 0) { return this; }

        Line currentLine = conversation.Lines[conversation.Lines.Count - 1];

        if (currentLine.Choices.Count == 0) { return this; }

        Choice currentChoice = currentLine.Choices[currentLine.Choices.Count - 1];

        // Assign 
        currentChoice.QuestID = inQuestID;

        return this;

    }




    public Conversation GetConversation()
    {
        return conversation;
    }


    // Static Dialogue Member
    public static Dialogue New(string id, List<Condition> conditions)
    {
        // Pass instance id to initializer conversation idLabel
        return new Dialogue(id, conditions);

    }

    public static Dialogue New(string id, List<Condition> conditions, Action callback)
    {
        // Pass instance id to initializer conversation idLabel
        return new Dialogue(id, conditions, callback);

    }

    // Overload without conditions
    public static Dialogue New(string id)
    {
        // Pass instance id to initializer conversation idLabel
        return new Dialogue(id);

    }

    public static Dialogue New(string id, Action callback)
    {
        // Pass instance id to initializer conversation idLabel
        return new Dialogue(id, callback);

    }


}
