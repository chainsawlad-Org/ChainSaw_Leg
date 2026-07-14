using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class TVDialogue : MonoBehaviour, IInteractable
{
    private GameStateMachine gameStateMachine;
    private DialogueRuntimeRegistry runtimeRegistry;
    private IRuntimeErrorLogger errorLogger;

    [Inject]
    public void Construct(
        GameStateMachine gameStateMachine,
        DialogueRuntimeRegistry runtimeRegistry,
        IRuntimeErrorLogger errorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.runtimeRegistry = runtimeRegistry;
        this.errorLogger = errorLogger;
    }

    public string GetInteractionPrompt() => "Press [E] to talk";

    public bool CanInteract()
    {
        IDialogueRuntime dialogueManager = runtimeRegistry.Current;
        return dialogueManager != null && !dialogueManager.IsActive;
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        StartDialogue(destroyCancellationToken).Forget();
    }

    private async UniTask StartDialogue(System.Threading.CancellationToken cancellationToken)
    {
        try
        {
            var events = DialogueLibrary.TVDialogue();

            await gameStateMachine.PushOverlay<DialoguePhase>(phase =>
            {
                phase.Configure(new DialogueRequest(
                    events,
                    DialogueType.RPG,
                    transform,
                    cancellationToken));
            });

            if (gameStateMachine.IsTopOverlay<DialoguePhase>())
                await gameStateMachine.PopOverlay();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, nameof(NpcDialogue));
        }
    }
}