using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class QuestDataBridge : MonoBehaviour
{


    [SerializeField]
    private QuestDataBridge questDataAsset;

    private List<Quest> quests = new List<Quest>();


    private void Start()
    {
        quests = questDataAsset.Quests;

    }

    public void AddConversation(Quest inConversation)
    {
        quests.Add(inConversation);
    }


    public void AddConversations(List<Quest> inConversations)
    {
        foreach (Quest conversation in inConversations)
        {
            quests.Add(conversation);
        }
    }

    public Quest GetDialogueById(string inIDLabel)
    {

        foreach (Quest quest in quests)
        {
            if (inIDLabel == quest.IDLabel)
            {
                return quest;
            }
        }

        return default;

    }


    public List<Quest> Quests { get => quests; set => quests = value; }



}