using UnityEngine;
using UnityEngine.Events;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string closedPrompt = "Press E to open";
    [SerializeField] private string openedPrompt = "Press E to close";
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;
    [SerializeField] private UnityEvent onLockedInteraction;

    public string GetInteractionPrompt()
    {
        if (isLocked)
        {
            return "Locked";
        }

        return isOpen ? openedPrompt : closedPrompt;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (isLocked)
        {
            onLockedInteraction?.Invoke();
            return;
        }

        isOpen = !isOpen;

        if (animator != null)
        {
            if (isOpen)
            {
                animator.SetTrigger(openTriggerName);
            }
            else
            {
                animator.SetTrigger(closeTriggerName);
            }
        }

        if (isOpen)
        {
            onOpened?.Invoke();
        }
        else
        {
            onClosed?.Invoke();
        }
    }

    public void SetLocked(bool value)
    {
        isLocked = value;
    }

    public void SetOpen(bool value)
    {
        isOpen = value;
    }
}