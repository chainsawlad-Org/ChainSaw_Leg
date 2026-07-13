using UnityEngine;

public class PlayerInteractionExecutor : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerInteractionDetector interactionDetector;
    [SerializeField] private bool isInteractionEnabled = true;

    private void Awake()
    {
        if (inputHandler == null)
        {
            inputHandler = GetComponent<PlayerInputHandler>();
        }

        if (interactionDetector == null)
        {
            interactionDetector = GetComponent<PlayerInteractionDetector>();
        }
    }

    private void Update()
    {
        if (!isInteractionEnabled || inputHandler == null || interactionDetector == null)
        {
            return;
        }

        if (!inputHandler.InteractPressed)
        {
            return;
        }

        inputHandler.ConsumeInteract();

        IInteractable target = interactionDetector.CurrentTarget;

        if (target == null || !target.CanInteract())
        {
            return;
        }

        target.Interact();
    }

    public void SetInteractionEnabled(bool value)
    {
        isInteractionEnabled = value;
    }
}