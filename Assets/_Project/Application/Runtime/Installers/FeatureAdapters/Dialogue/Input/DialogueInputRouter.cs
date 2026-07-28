using UnityEngine;
using Zenject;

public class DialogueInputRouter : MonoBehaviour
{
    private InputService inputService;
    private DialogueRuntimeRegistry runtimeRegistry;

    [Inject]
    public void Construct(
        InputService inputService,
        DialogueRuntimeRegistry runtimeRegistry)
    {
        this.inputService = inputService;
        this.runtimeRegistry = runtimeRegistry;
    }

    private void Update()
    {
        if (runtimeRegistry == null || inputService == null)
            return;

        IDialogueRuntime dialogueManager = runtimeRegistry.Current;

        if (dialogueManager == null)
            return;

        if (!dialogueManager.IsActive)
            return;

        if (dialogueManager.State == DialogueState.Choosing)
        {
            if (inputService.PreviousPressed)
            {
                inputService.ConsumePrevious();
                dialogueManager.SelectPreviousChoice();
            }

            if (inputService.NextPressed)
            {
                inputService.ConsumeNext();
                dialogueManager.SelectNextChoice();
            }
        }

        if (!inputService.SubmitPressed)
            return;

        inputService.ConsumeSubmit();

        dialogueManager.Submit();
    }
}
