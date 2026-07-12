using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CutsceneTrigger : MonoBehaviour
{
    private GameStateMachine gameStateMachine;
    private IRuntimeErrorLogger errorLogger;

    private bool triggered;

    [Inject]
    public void Construct(
        GameStateMachine gameStateMachine,
        IRuntimeErrorLogger errorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.errorLogger = errorLogger;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        RunCutsceneAsync(destroyCancellationToken).Forget();
    }

    private async UniTask RunCutsceneAsync(CancellationToken cancellationToken)
    {
        try
        {
            var events = new List<IDialogueEvent>()
            {
                new TypewriterEvent { text = "Враг: Я ждал тебя...", speed = 0.04f },
                new TypewriterEvent { text = "Игрок: Похоже, ты не устал ждать.", speed = 0.04f },
                new TypewriterEvent { text = "Враг: Сегодня всё закончится.", speed = 0.04f },
                new TypewriterEvent { text = "Игрок: Да. Прямо сейчас.", speed = 0.04f },
            };

            await gameStateMachine.PushOverlay<DialoguePhase>(phase =>
            {
                phase.Configure(new DialogueRequest(
                    events,
                    DialogueType.Cutscene,
                    transform,
                    cancellationToken));
            });

            if (gameStateMachine.IsTopOverlay<DialoguePhase>())
                await gameStateMachine.PopOverlay();

            await gameStateMachine.ReplaceMainAsync<BattlePhase>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            triggered = false;
            errorLogger.LogException(exception, nameof(CutsceneTrigger));
        }
    }
}
