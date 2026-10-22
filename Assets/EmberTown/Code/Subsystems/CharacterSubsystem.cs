using UnityEngine;

public class CharacterSubsystem : MonoBehaviour
{

    private static CharacterSubsystem instance;


    public static CharacterSubsystem Call()
    {
        return instance;
    }

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

    // States Data
    [SerializeField]
    private Names characterName;

    // Public Members
    public Names Name { get => characterName; set => characterName = value; }


    // State Enums
    public enum Names
    {
        Hero,
        Amira,
        Unknown,

    }

}



