using System.Collections.Generic;
using UnityEngine;


public class PlayerQuestsDatabase : MonoBehaviour
{

    private static PlayerQuestsDatabase instance;
    public static PlayerQuestsDatabase Call()
    {
        return instance;
    }


    private Dictionary<QuestsDB.IDs, Quest> playerQuests = new Dictionary<QuestsDB.IDs, Quest>();



    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }


        instance = this;
        DontDestroyOnLoad(gameObject);

    }





}