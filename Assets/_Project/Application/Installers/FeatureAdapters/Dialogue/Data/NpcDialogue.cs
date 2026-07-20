using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class NpcDialogue : MonoBehaviour, IInteractable
{
    private enum DialogueContent
    {
        Test,
        TV,
        Bed
    }

    [SerializeField] private DialogueContent dialogueContent;
    [SerializeField] private string interactionPrompt = "Press E to talk";

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

    public string GetInteractionPrompt() => interactionPrompt;

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
            var events = dialogueContent switch
            {
                DialogueContent.TV => DialogueLibrary.TVDialogue(),
                DialogueContent.Bed => DialogueLibrary.BedDialogue(),
                _ => DialogueLibrary.TestDialogue()
            };

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
        catch (OperationCanceledException)
        {
            // A main phase transition closes DialoguePhase before Unity destroys this scene object.
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, nameof(NpcDialogue));
        }
    }
}
