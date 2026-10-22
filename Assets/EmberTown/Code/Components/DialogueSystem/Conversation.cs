using System;
using System.Collections.Generic;

/// <summary>
/// Conversation is the container of lines and choices
/// </summary>
public class Conversation
{
    private List<Line> lines = new List<Line>();

    private List<Condition> conditions = new List<Condition>();

    private Action callbackEvent;

    private string idLabel;

    public List<Line> Lines { get => lines; set => lines = value; }

    public string IDLabel { get => idLabel; set => idLabel = value; }
    public List<Condition> Conditions { get => conditions; set => conditions = value; }
    public Action CallbackEvent { get => callbackEvent; set => callbackEvent = value; }
}









