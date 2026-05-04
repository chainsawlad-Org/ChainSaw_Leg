using System.Collections.Generic;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        var events = new List<IDialogueEvent>()
        {
            new TypewriterEvent { text = "Враг: Я ждал тебя...", speed = 0.04f },
            new TypewriterEvent { text = "Игрок: Похоже, ты не устал ждать.", speed = 0.04f },

            new TypewriterEvent { text = "Враг: Сегодня всё закончится.", speed = 0.04f },
            new TypewriterEvent { text = "Игрок: Да. Прямо сейчас.", speed = 0.04f },
        };

        DialogueManager.Instance.StartDialogue(events, DialogueType.Cutscene, transform);
    }
}