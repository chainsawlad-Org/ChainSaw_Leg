using NUnit.Framework;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueNode currentNode;
    private DialogueType currentType;
    private Transform currentSpeaker;

    public bool IsActive { get; private set; }

    public bool HasChoices =>
        currentNode != null &&
        currentNode.choices != null &&
        currentNode.choices.Count > 0;

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueData data, DialogueType type, Transform speaker)
    {
        if (data == null || data.startNode == null) return;

        IsActive = true;
        currentNode = data.startNode;
        currentType = type;
        currentSpeaker = speaker;

        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }

        switch (currentType)
        {
            case DialogueType.Bubble:
                DialogueUI_Bubble.Instance.Show(currentNode.text, currentSpeaker);
                break;
            case DialogueType.RPG:
                DialogueUI_RPG.Instance.Show(currentNode);
                break;
            case DialogueType.Cutscene:
                DialogueUI_Cutscene.Instance.Show(currentNode);
                break;
        }
    }

    public void Next()
    {
        if (!IsActive || HasChoices) return;

        if (currentNode.nextNode != null)
        {
            currentNode = currentNode.nextNode;
            ShowCurrentNode();
        }
        else
        {
            EndDialogue();
        }
    }


    public void Choose(int index)

    {
        if (!HasChoices) return;

        currentNode = currentNode.choices[index].nextNode;
        ShowCurrentNode();
    }

    private void EndDialogue()
    {
        IsActive = false;
        currentNode = null;

        DialogueUI_Bubble.Instance.Hide();
        DialogueUI_RPG.Instance.Hide();
        DialogueUI_Cutscene.Instance.Hide();
    }
}