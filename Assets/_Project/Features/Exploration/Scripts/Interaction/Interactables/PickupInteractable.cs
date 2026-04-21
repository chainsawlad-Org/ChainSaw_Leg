using UnityEngine;
using UnityEngine.Events;

public class PickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Press E to pick up";
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private UnityEvent onPickedUp;

    private bool isCollected;

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public bool CanInteract()
    {
        return !isCollected;
    }

    public void Interact()
    {
        if (isCollected)
        {
            return;
        }

        isCollected = true;
        onPickedUp?.Invoke();

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

    public void ResetCollected()
    {
        isCollected = false;
    }
}