using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BubbleTrigger : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string text = "Эй! Не проходи мимо!";

    [Header("Bubble Anchor")]
    [SerializeField] private Transform bubbleAnchor;
    private DialogueRuntimeRegistry runtimeRegistry;

    [Inject]
    public void Construct(DialogueRuntimeRegistry runtimeRegistry)
    {
        this.runtimeRegistry = runtimeRegistry;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        DialogueManager dialogueManager = runtimeRegistry.Current;

        if (dialogueManager == null)
            return;

        var events = new List<IDialogueEvent>()
        {
            new ShowTextEvent() { text = text },
            new DelayEvent() { duration = 2f},
        };

        Transform targetAnchor = bubbleAnchor != null ? bubbleAnchor : transform;

        dialogueManager.StartDialogue(
            events,
            DialogueType.Bubble,
            targetAnchor
        );
    }
}
