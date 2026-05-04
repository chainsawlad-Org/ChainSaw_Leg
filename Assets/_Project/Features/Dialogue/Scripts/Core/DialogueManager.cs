using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private DialogueUI_RPG rpgUI;
    [SerializeField] private DialogueUI_Bubble bubbleUI;
    [SerializeField] private DialogueUI_Cutscene cutsceneUI;

    private Queue<IDialogueEvent> eventQueue = new();

    private DialogueType currentType;
    private Transform currentSpeaker;

    public DialogueState State { get; private set; } = DialogueState.Idle;
    public bool IsActive => State != DialogueState.Idle;

    public DialogueType CurrentType => currentType;

    private Coroutine typingCoroutine;
    private bool isTyping;
    public string CurrentFullText { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(List<IDialogueEvent> events, DialogueType type, Transform speaker = null)
    {
        eventQueue.Clear();

        foreach (var e in events)
            eventQueue.Enqueue(e);

        currentType = type;
        currentSpeaker = speaker;

        SetState(DialogueState.Playing);

        ShowUI();
        ProcessNextEvent();
    }

    private void ShowUI()
    {
        rpgUI.Hide();
        bubbleUI.Hide();
        cutsceneUI.Hide();

        switch (currentType)
        {
            case DialogueType.RPG:
                rpgUI.ShowRoot();
                break;

            case DialogueType.Bubble:
                bubbleUI.SetTarget(currentSpeaker);
                bubbleUI.ShowRoot();
                break;

            case DialogueType.Cutscene:
                cutsceneUI.ShowRoot();
                break;
        }
    }

    public void ProcessNextEvent()
    {
        if (eventQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        var e = eventQueue.Dequeue();
        e.Execute(this);
    }

    public void Submit()
    {
        if (State == DialogueState.Typing)
        {
            SkipTyping();
            return;
        }

        if (State == DialogueState.WaitingInput)
        {
            ProcessNextEvent();
        }
    }

    public void Choose(int index, List<DialogueChoice> choices)
    {
        if (choices == null || index < 0 || index >= choices.Count)
            return;

        var next = choices[index].nextNode;

        if (next == null)
        {
            ProcessNextEvent();
            return;
        }

        var newEvents = ConvertNodeToEvents(next);

        eventQueue.Clear();

        foreach (var e in newEvents)
            eventQueue.Enqueue(e);

        ProcessNextEvent();
    }

    private List<IDialogueEvent> ConvertNodeToEvents(DialogueNode start)
    {
        var events = new List<IDialogueEvent>();

        DialogueNode current = start;

        while (current != null)
        {
            events.Add(new TypewriterEvent { text = current.text, speed = 0.05f });

            if (current.choices != null && current.choices.Count > 0)

            {
                events.Add(new ChoiceEvent { choices = current.choices });
                break;
            }
            current = current.nextNode;
        }
        return events;
    }

    public void StartTyping(string text, float speed)
    {
        CurrentFullText = text;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine(text, speed));
    }

    private IEnumerator TypeRoutine(string text, float speed)
    {
        isTyping = true;
        SetState(DialogueState.Typing);

        string current = "";

        foreach (char c in text)
        {
            current += c;
            ShowText(current);

            yield return new WaitForSeconds(speed);
        }

        isTyping = false;
        SetState(DialogueState.WaitingInput);
    }

    public void SkipTyping()
    {
        if (!isTyping)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        ShowText(CurrentFullText);

        isTyping = false;
        SetState(DialogueState.WaitingInput);
    }

    public void ShowText(string text)
    {
        switch (currentType)
        {
            case DialogueType.RPG:
                rpgUI.ShowText(text);
                break;

            case DialogueType.Bubble:
                bubbleUI.ShowText(text);
                break;

            case DialogueType.Cutscene:
                cutsceneUI.ShowText(text);
                break;
        }
    }

    public void ShowChoices(List<DialogueChoice> choices)
    {
        if (currentType == DialogueType.RPG)
        {
            rpgUI.ShowChoices(choices);
        }
    }

    public void SetState(DialogueState state)
    {
        State = state;
    }

    private void EndDialogue()
    {
        SetState(DialogueState.Idle);

        rpgUI.Hide();
        bubbleUI.Hide();
        cutsceneUI.Hide();

        currentSpeaker = null;
    }
}