using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleTrigger : MonoBehaviour
{
    [TextArea] public string text = "Эй! Не проходи мимо!";

    [Header("Bubble Anchor")]
    [SerializeField] private Transform bubbleAnchor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var events = new List<IDialogueEvent>()
        {
            new ShowTextEvent() { text = text },
            new DelayEvent() { duration = 2f},
        };

        DialogueManager.Instance.StartDialogue(
            events,
            DialogueType.Bubble,
            bubbleAnchor
        );
    }
}
