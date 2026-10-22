using System.Collections.Generic;
using UnityEngine;

public class DialoguePlayer : MonoBehaviour
{

    public Conversation currentConversation { get; private set; }
    public Line currentLine { get; private set; }
    public int currentLineIndex { get; private set; }
    public List<Choice> currentChoices { get; private set; }
    public Choice selectedChoice { get; private set; }

    public void StartConversation(Conversation conversation)
    {
        // Guard conversation data doesn't exist
        if (conversation == null || conversation.Lines == null || conversation.Lines.Count == 0)
        {
            Debug.Log("Attempted to start a null or empty conversation.");
            End();
            return;
        }

        // Set Active Conversation State
        State = States.Active;

        // Reset current line index to 0;
        currentLineIndex = 0;

        currentConversation = conversation;

        // Start Conversation Callback
        currentConversation.CallbackEvent?.Invoke();

        // Set current line index to current line index
        currentLine = conversation.Lines[currentLineIndex];



        // Check if starting index has choices and set Substates
        if (lineHasChoices(out List<Choice> choices))
        {
            SubState = SubStates.WaitingForChoice;
            currentChoices = choices;
        }
        else
        {
            SubState = SubStates.None;
            currentChoices = null;
        }



        // DialogueSubsystem.Call().State = DialogueSubsystem.States.Active;
        SendConversationData();


    }

    private void SendConversationData()
    {

        TArgs<States> stateArgs = new TArgs<States>("State", State);

        TArgs<Conversation> convArgs = new TArgs<Conversation>("Conversation", currentConversation);

        TArgs<Line> lineArgs = new TArgs<Line>("Line", currentLine);

        TArgs<List<Choice>> choiceArgs = new TArgs<List<Choice>>("Choices", currentChoices);

        DialogueSubsystem.Call().EmitAvailableDialogueEvent(new DArgs(convArgs, lineArgs, choiceArgs, stateArgs));

    }

    public bool lineHasChoices(out List<Choice> choices)
    {

        choices = null;

        if (currentLine == null || currentLine.Choices == null || currentLine.Choices.Count == 0)
        {
            return false;
        }


        choices = currentLine.Choices;
        return true;
    }




    public void Advance()
    {
        if (State != States.Active || currentConversation == null || currentLine == null) { return; }

        if (SubState == SubStates.WaitingForChoice) { return; }


        // End if index reached lines array count
        if (currentLineIndex + 1 >= currentConversation.Lines.Count)
        {
            End();
            return;
        }

        // Advance conversation
        currentLineIndex++;
        currentLine = currentConversation.Lines[currentLineIndex];

        // Check if line has choices
        if (lineHasChoices(out List<Choice> choices))
        {
            SubState = SubStates.WaitingForChoice;
            currentChoices = choices;

        }
        else
        {
            SubState = SubStates.None;
            currentChoices = null;
        }

        SendConversationData();
    }


    public void Choose(int index)
    {
        if (currentChoices == null || index < 0 || index >= currentChoices.Count)
        {
            return;
        }


        currentConversation = currentChoices[index].Conversation;
        StartConversation(currentConversation);


        Debug.Log("I've been choosen!");


    }

    public void End()
    {
        currentLineIndex = 0;

        if (State != States.Ended)
        {

            State = States.Ended;
            SubState = SubStates.None;
            currentChoices = null;
        }

        SendConversationData();
    }



    public enum States
    {
        None,
        InRange,
        Active,
        Ended,

    }

    public enum SubStates
    {
        None,
        WaitingForChoice,
        Interrupted,

    }

    public SubStates SubState;
    public States State;



}