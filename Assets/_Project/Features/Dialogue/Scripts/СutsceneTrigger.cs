using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CutsceneTrigger : MonoBehaviour
{
    [Inject] private GameStateMachine gameStateMachine;

    private bool triggered;

    private async void OnTriggerEnter2D(Collider2D other)
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

        var tcs = new UniTaskCompletionSource();

        void OnFinished()
        {
            DialogueManager.Instance.DialogueFinished -= OnFinished;
            tcs.TrySetResult();
        }

        DialogueManager.Instance.DialogueFinished += OnFinished;

        DialogueManager.Instance.StartDialogue(
            events,
            DialogueType.Cutscene,
            transform);

        // Ждем окончания катсцены
        await tcs.Task;

        // После этого запускаем бой
        await gameStateMachine.ReplaceMain<BattlePhase>();
    }
}