using UnityEngine;
using Zenject;

public class DialogueInputRouter : MonoBehaviour
{
    private InputService inputService;

    [Inject]
    public void Construct(InputService inputService)
    {
        this.inputService = inputService;
    }

    private void Update()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;

        if (dialogueManager == null || inputService == null)
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
