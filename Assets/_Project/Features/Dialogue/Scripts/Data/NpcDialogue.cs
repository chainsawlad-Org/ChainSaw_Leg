using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class NpcDialogue : MonoBehaviour, IInteractable
{
    [Inject] private GameStateMachine gameStateMachine;

    public string GetInteractionPrompt() => "Press [E] to talk";

    public bool CanInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        return dialogueManager != null && !dialogueManager.IsActive;
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null)
            return;

        StartDialogue().Forget();
    }

    private async UniTask StartDialogue()
    {
        var events = DialogueLibrary.TestDialogue();

        await gameStateMachine.PushOverlay<DialoguePhase>(phase =>
        {
            phase.Configure(new DialogueRequest
            {
                Events = events,
                Type = DialogueType.RPG,
                Speaker = transform
            });
        });

        if (gameStateMachine.IsTopOverlay<DialoguePhase>())
            await gameStateMachine.PopOverlay();
    }
}
