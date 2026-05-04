using System.Collections.Generic;
using UnityEngine;

public class NpcDialogue : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt() => "Press [E] to talk";


    public bool CanInteract()
    {
        return !DialogueManager.Instance.IsActive;
    }

    public void Interact()
    {
        var events = DialogueLibrary.TestDialogue();

        DialogueManager.Instance.StartDialogue(events, DialogueType.RPG, transform);
    }
}
