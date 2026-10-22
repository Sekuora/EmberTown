using System;
using System.Collections.Generic;

/// <summary>
/// Represents a dialogue text line
/// </summary>
public class Line
{
    // Speaker Name
    public CharacterSubsystem.Names Speaker;

    // Dialogue Text
    public string Text;

    // Conditions List
    private List<Condition> conditions = new List<Condition>();

    // Choices List
    private List<Choice> choices = new List<Choice>();

    // Line Callback Event
    private Action callbackEvent;

    private string questID;


    // Public
    public List<Choice> Choices { get => choices; set => choices = value; }
    public List<Condition> Conditions { get => conditions; set => conditions = value; }
    public Action CallbackEvent { get => callbackEvent; set => callbackEvent = value; }
    public string QuestID { get => questID; set => questID = value; }
}