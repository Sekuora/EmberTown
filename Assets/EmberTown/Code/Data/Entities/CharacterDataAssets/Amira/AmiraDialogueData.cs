using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Reflection;

/// <summary>
/// Inherit DialogueDataBridge Reference from parent 
/// and Call to AddConversationData method on Awake.
/// </summary>
public class AmiraDialogueData : DialogueDataAsset
{


    public Conversation Root_Intro01()
    {

        List<Condition> conditions = new List<Condition>
        {
            new
            Condition
            (
                Condition.Types.MeetNPC,
                CharacterSubsystem.Names.Amira,
                false
            )
        };

        return
         Dialogue.New(nameof(ConversationMapID.AmiraIntroduction_01), conditions, () =>
         {
             PlayerFlagsDatabase.Call().SetKnowNPC(CharacterSubsystem.Names.Amira, true);

         })
        .Speaker(CharacterSubsystem.Names.Amira)
        .Line("Hello, are you the new guy?")

        .Line("It's nice to meet you")
        .Quest_Line(nameof(QuestsDB.IDs.HelpAmiraQuestline01))

        .Choice("Nice to meet you too", Intro02)
        .GetConversation();

    }

    public Conversation Intro02()
    {
        return
        Dialogue.New(nameof(ConversationMapID.AmiraIntroduction_02))
        .Speaker(CharacterSubsystem.Names.Hero)
        .Line("I'm no friendo D:")
        .Speaker(CharacterSubsystem.Names.Amira)
        .Line("In any case I don't fear you")
        .Line("I r.. really ")
        .Line("I don't fear you!!")
        .Choice("I'm gonna eat you wahahaha!", Intro03)
        .GetConversation();

    }

    public Conversation Intro03()
    {
        return
        Dialogue.New(nameof(ConversationMapID.AmiraIntroduction_03))
        .Speaker(CharacterSubsystem.Names.Amira)
        .Line("Waahhhh!!")
        .GetConversation();
    }

    public Conversation Root_Intro04()
    {
        List<Condition> conditions = new List<Condition>
        {
            new
            Condition
            (
                Condition.Types.MeetNPC,
                CharacterSubsystem.Names.Amira,
                true
            )
        };

        return
        Dialogue.New(nameof(ConversationMapID.AmiraIntroduction_04), conditions)
        .Speaker(CharacterSubsystem.Names.Amira)
        .Line("Waahhhh!!")
        .GetConversation();
    }


}



public enum ConversationMapID
{
    AmiraIntroduction_01,
    AmiraIntroduction_02,
    AmiraIntroduction_03,
    AmiraIntroduction_04,

}

// public static class ConversationID
// {
//     public const string Intro_01 = "AmiraIntroduction_01";

// }

