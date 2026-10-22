using System;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public enum State
    {
        Inactive,
        Active,
        Completed
    }


    // Quest IDs Enum are in each NPCs Quest Data Asset
    public string IDLabel { get; private set; }

    // The current quest state is defined by the quest state enum
    public State currentState { get; set; }

    // Multiple Conditions 
    public List<Condition> Conditions { get; private set; }

    // Single Condition
    public Condition Condition { get; private set; }

    // Action Events
    public Action EventQuestComplete { get; private set; }
    public Action EventQuestStart { get; private set; }

    //  Initializers
    // Multiple Conditions
    public Quest(string inId, List<Condition> inConditions, Action inEventQuestComplete)
    {

        IDLabel = inId;
        currentState = State.Inactive;
        Conditions = inConditions;
        EventQuestComplete = inEventQuestComplete;

    }

    // Single Condition
    public Quest(string inId, Condition inCondition, Action inEventQuestComplete)
    {

        IDLabel = inId;
        currentState = State.Inactive;
        Condition = inCondition;
        EventQuestComplete = inEventQuestComplete;


    }

}


