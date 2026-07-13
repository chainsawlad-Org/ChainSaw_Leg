using UnityEngine;

public class InteractionHighlightPresenter : MonoBehaviour
{
    [SerializeField] private PlayerInteractionDetector interactionDetector;

    private IInteractable currentInteractable;
    private IInteractionHighlightTarget currentHighlightTarget;

    private void OnEnable()
    {
        if (interactionDetector != null)
        {
            interactionDetector.CurrentTargetChanged += OnCurrentTargetChanged;
            OnCurrentTargetChanged(interactionDetector.CurrentTarget);
        }
    }

    private void OnDisable()
    {
        if (interactionDetector != null)
        {
            interactionDetector.CurrentTargetChanged -= OnCurrentTargetChanged;
        }

        ClearCurrentHighlight();
    }

    private void OnCurrentTargetChanged(IInteractable target)
    {
        if (ReferenceEquals(currentInteractable, target))
        {
            return;
        }

        ClearCurrentHighlight();

        currentInteractable = target;
        currentHighlightTarget = ResolveHighlightTarget(target);

        if (currentHighlightTarget != null)
        {
            currentHighlightTarget.SetHighlighted(true);
        }
    }

    private void ClearCurrentHighlight()
    {
        if (currentHighlightTarget != null)
        {
            currentHighlightTarget.SetHighlighted(false);
        }

        currentInteractable = null;
        currentHighlightTarget = null;
    }

    private static IInteractionHighlightTarget ResolveHighlightTarget(IInteractable interactable)
    {
        if (!(interactable is MonoBehaviour monoBehaviour))
        {
            return null;
        }

        MonoBehaviour[] behaviours = monoBehaviour.GetComponents<MonoBehaviour>();

        for (int index = 0; index < behaviours.Length; index++)
        {
            if (behaviours[index] is IInteractionHighlightTarget highlightTarget)
            {
                return highlightTarget;
            }
        }

        return null;
    }
}