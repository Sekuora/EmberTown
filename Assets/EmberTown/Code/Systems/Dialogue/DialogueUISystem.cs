using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class DialogueUISystem : DialogueUITemplate
{
    private DialoguePlayer dsPlayer;

    // Dialogue Data
    private List<Choice> currentChoices;

    private void Start()
    {
        // Check if dialogue subsystem has been found
        if (!DialogueSubsystem.Call()) { Debug.Log("No Dialogue Subsystem Instance found at: " + name); return; }
        DialogueSubsystem.Call().OnAvailableDialogueEvent += DialogueEventCallback;

    }

    private void OnDisable()
    {
        DialogueSubsystem.Call().OnAvailableDialogueEvent -= DialogueEventCallback;
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void DialogueEventCallback(DArgs args)
    {
        dsPlayer = DialogueSubsystem.Call().DSPlayer;
        if (dsPlayer == null) { return; }


        // Unpack args
        Line line = args.GetByString<Line>("Line");

        DialoguePlayer.States state = args.GetByString<DialoguePlayer.States>("State");

        currentChoices = args.GetByString<List<Choice>>("Choices");


        // Handle conversation according to DialogueSubsystem conversation state
        switch (state)
        {
            case DialoguePlayer.States.Active:

                if (line == null)
                {
                    ShowDialogUI(false);
                    return;
                }

                if (currentChoices == null)
                {
                    choicesContainer.Clear();

                }

                // Set Speaker Name
                speakerName.text = line.Speaker.ToString();
                dialogue.text = line.Text;
                AddChoiceButtons();
                ShowDialogUI(true);

                break;

            case DialoguePlayer.States.Ended:
                ShowDialogUI(false);
                break;


            default:
                ShowDialogUI(false);
                break;

        }
    }

    private void AddChoiceButtons()
    {
        if (currentChoices == null) { return; }
        choicesContainer.Clear();


        // Iterate current choices array
        for (int i = 0; i < currentChoices.Count; i++)
        {
            int choiceIndex = i;

            // Query button element from choice buttom template instance
            VisualElement buttonTemplateInst = choiceButtonTemplate.Instantiate();
            Button button = buttonTemplateInst.Q<Button>("ChoiceButton");

            // Assing label from the current choices array;
            button.text = currentChoices[choiceIndex].Label;

            // Call DSPlayer to handle button interaction with mouse
            button.clicked += () => DialogueSubsystem.Call().DSPlayer.Choose(choiceIndex);

            choicesContainer.Add(buttonTemplateInst);

        }
    }


}