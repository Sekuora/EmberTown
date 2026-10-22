using System;
using UnityEngine;

public class DialogueSubsystem : MonoBehaviour
{


    [SerializeField]
    private DialoguePlayer dsPlayer;

    private static DialogueSubsystem instance;

    // On Dialogue Event
    public event Action<DArgs> OnDialogueDataEvent;

    // On Dialogue Event
    public event Action<DArgs> OnAvailableDialogueEvent;

    // Dialogue Event Emitter
    public void EmitDialogueDataEvent(DArgs args)
    {

        Debug.Log("Dialogue Emitted");
        OnDialogueDataEvent?.Invoke(args);
    }

    public void EmitAvailableDialogueEvent(DArgs args)
    {
        OnAvailableDialogueEvent?.Invoke(args);
    }


    // Call Instance
    public static DialogueSubsystem Call()
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




    // Getter / Setter
    public DialoguePlayer DSPlayer { get => dsPlayer; }
}



