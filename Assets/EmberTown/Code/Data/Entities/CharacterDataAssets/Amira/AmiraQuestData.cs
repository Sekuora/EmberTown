using System;
using System.Collections.Generic;
using UnityEngine;


public class AmiraQuestData : QuestDataAsset
{

    public Quest Quest_AmiraWood()
    {
        List<Condition> completionConditions = new List<Condition>
        {

            new Condition(ItemIDsDatabase.ItemId.TestItem, Condition.Types.RequiresItemAmount, 50)

        };

        Action QuestCompleteCallbackEvent = () =>
        {
            Debug.Log("You completed the quest");
        };

        return new Quest(nameof(QuestsDB.IDs.HelpAmiraQuestline01), completionConditions, QuestCompleteCallbackEvent);
    }


}