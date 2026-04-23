using UnityEngine;

public class NpcDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private DialogueType type;

    public string GetInteractionPrompt()
    {
        return "Press [E] to talk";
    }

    public bool CanInteract()
    {
        return dialogue != null && !DialogueManager.Instance.IsActive;
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogue, type, transform);
    }
}
