using System;

/// <summary>
/// A choice is a reference to another conversation
/// </summary>
public class Choice
{
    // Choice option button text to display in UI
    private string label;

    // Point to a conversation reference
    private Conversation conversation;

    // Choice callback action
    private Action callbackEvent;

    // Quset ID : ? = Enum allows null values
    private string questId;

    // Public
    public string Label { get => label; set => label = value; }
    public Conversation Conversation { get => conversation; set => conversation = value; }
    public Action CallbackEvent { get => callbackEvent; set => callbackEvent = value; }
    public string QuestID { get => questId; set => questId = value; }

}

