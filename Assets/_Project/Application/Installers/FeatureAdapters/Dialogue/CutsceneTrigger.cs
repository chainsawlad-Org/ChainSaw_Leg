using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private string encounterId;
    [SerializeField] private GameObject defeatedObject;

    private GameStateMachine gameStateMachine;
    private IBattleSceneTransitionService battleSceneTransitionService;
    private BattleSessionService battleSessionService;
    private IRuntimeErrorLogger errorLogger;

    private bool triggered;

    [Inject]
    public void Construct(
        GameStateMachine gameStateMachine,
        IBattleSceneTransitionService battleSceneTransitionService,
        BattleSessionService battleSessionService,
        IRuntimeErrorLogger errorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.battleSceneTransitionService = battleSceneTransitionService;
        this.battleSessionService = battleSessionService;
        this.errorLogger = errorLogger;
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(encounterId))
        {
            errorLogger.LogException(
                new InvalidOperationException("Cutscene encounter ID is required."),
                nameof(CutsceneTrigger));
            enabled = false;
            return;
        }

        if (!battleSessionService.IsEncounterDefeated(encounterId))
            return;

        if (defeatedObject != null)
            defeatedObject.SetActive(false);

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        Rigidbody2D playerBody = other.attachedRigidbody;
        Vector2 returnPosition = playerBody != null
            ? playerBody.position
            : (Vector2)other.transform.position;
        battleSessionService.BeginEncounter(encounterId, returnPosition.x, returnPosition.y);
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

            battleSceneTransitionService.RequestEnterBattle();
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
