using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using Zenject;

public class NpcDialogue : MonoBehaviour, IInteractable
{
    [Inject] private GameStateMachine gameStateMachine;

    public string GetInteractionPrompt() => "Press [E] to talk";

    public bool CanInteract()
    {
        return !DialogueManager.Instance.IsActive;
    }

    public void Interact()
    {
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
    }
}
