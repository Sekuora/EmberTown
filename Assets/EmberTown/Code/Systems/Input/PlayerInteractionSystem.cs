using System;
using UnityEngine;

public class PlayerInteractionSystem : MonoBehaviour
{
    // Component References
    private InputDataBridge input;
    private Conversation currentConversation;

    private DialoguePlayer dsPlayer;


    private void Awake()
    {
        if (!TryGetComponent(out input)) { return; }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        DialogueSubsystem.Call().OnAvailableDialogueEvent += AvailableDialogueCallback;

        dsPlayer = DialogueSubsystem.Call().DSPlayer;


    }

    private void OnDisable()
    {
        DialogueSubsystem.Call().OnAvailableDialogueEvent -= AvailableDialogueCallback;
    }

    private void AvailableDialogueCallback(DArgs args)
    {

        currentConversation = args.GetByString<Conversation>("AvailableConversation");

    }

    // Update is called once per frame
    private void Update()
    {

        if (!input.InteractPressed) { return; }


        if (dsPlayer.State == DialoguePlayer.States.Active)
        {
            dsPlayer.Advance();
            return;
        }

        if (dsPlayer.State == DialoguePlayer.States.InRange && currentConversation != null)
        {
            dsPlayer.StartConversation(currentConversation);
        }

    }
}
