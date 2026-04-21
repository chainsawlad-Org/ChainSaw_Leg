using UnityEngine;
using UnityEngine.Events;

public class NpcDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Press E to talk";
    [SerializeField] private bool canRepeat = true;
    [SerializeField] private bool startsBusy;
    [SerializeField] private UnityEvent onInteract;

    private bool isBusy;
    private bool hasBeenUsed;

    private void Awake()
    {
        isBusy = startsBusy;
    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public bool CanInteract()
    {
        if (isBusy)
        {
            return false;
        }

        if (!canRepeat && hasBeenUsed)
        {
            return false;
        }

        return true;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        hasBeenUsed = true;
        onInteract?.Invoke();
    }

    public void SetBusy(bool value)
    {
        isBusy = value;
    }

    public void ResetUsage()
    {
        hasBeenUsed = false;
    }
}