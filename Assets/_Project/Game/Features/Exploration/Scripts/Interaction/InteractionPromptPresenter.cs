using TMPro;
using UnityEngine;

public class InteractionPromptPresenter : MonoBehaviour
{
    [SerializeField] private PlayerInteractionDetector interactionDetector;
    [SerializeField] private ExplorationInteractionConfig interactionConfig;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;

    private void OnEnable()
    {
        if (interactionDetector != null)
        {
            interactionDetector.CurrentTargetChanged += OnCurrentTargetChanged;
            Refresh(interactionDetector.CurrentTarget);
        }
        else
        {
            Hide();
        }
    }

    private void OnDisable()
    {
        if (interactionDetector != null)
        {
            interactionDetector.CurrentTargetChanged -= OnCurrentTargetChanged;
        }
    }

    private void OnCurrentTargetChanged(IInteractable target)
    {
        Refresh(target);
    }

    private void Refresh(IInteractable target)
    {
        if (target == null)
        {
            Hide();
            return;
        }

        string prompt = target.GetInteractionPrompt();

        if (string.IsNullOrWhiteSpace(prompt))
        {
            prompt = interactionConfig != null
                ? interactionConfig.DefaultPromptText
                : "Press E to interact";
        }

        Show(prompt);
    }

    private void Show(string value)
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
        }

        if (promptText != null)
        {
            promptText.text = value;
        }
    }

    private void Hide()
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }

        if (promptText != null)
        {
            promptText.text = string.Empty;
        }
    }
}