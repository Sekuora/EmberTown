using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
/// <summary>
/// Contains references that match the UI Document 
/// Visual Elements.
/// </summary>
/// 
[RequireComponent(typeof(PanelRenderer))]
public class DialogueUITemplate : MonoBehaviour
{


    private int uiVersion = 0;

    // References //
    [SerializeField]
    protected VisualTreeAsset choiceButtonTemplate;

    // Panel Renderer Component
    protected PanelRenderer panelRenderer;


    // UI Document References
    protected VisualElement dialogueContainer;
    protected Label speakerName;
    protected Label dialogue;
    protected VisualElement speaker1;
    protected VisualElement speaker2;
    protected VisualElement choicesContainer;


    // Public Members
    public VisualElement DialogueContainer { get => dialogueContainer; set => dialogueContainer = value; }
    public Label SpeakerNameText { get => speakerName; set => speakerName = value; }
    public Label DialogueText { get => dialogue; set => dialogue = value; }
    public VisualElement Speaker1 { get => speaker1; set => speaker1 = value; }
    public VisualElement Speaker2 { get => speaker2; set => speaker2 = value; }
    public VisualElement ChoicesContainer { get => choicesContainer; set => choicesContainer = value; }


    // Get Panel Renderer
    private void OnEnable()
    {
        // Get References
        if (TryGetComponent(out panelRenderer))
        {

            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }
        else
        {
            Debug.LogWarning("DialogueSystemUI - PanelRenderer Not Found");
        }

    }


    // Obtain UI Document Elements from Panel Renderer
    protected void OnUIReload(PanelRenderer render, VisualElement root, int version)
    {
        if (uiVersion == version) return;
        uiVersion = version;

        // Set References
        dialogueContainer = root.Q<VisualElement>("DialogueContainer");
        speakerName = root.Q<Label>("SpeakerName");
        dialogue = root.Q<Label>("Dialogue");
        speaker1 = root.Q<VisualElement>("Speaker1");
        speaker2 = root.Q<VisualElement>("Speaker2");
        choicesContainer = root.Q<VisualElement>("ChoicesContainer");

        ShowDialogUI(false);
    }

    protected void ShowDialogUI(bool visible)
    {
        if (dialogueContainer == null) return;

        dialogueContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

    }
}
