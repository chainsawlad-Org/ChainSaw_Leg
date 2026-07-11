using UnityEngine;

public class DialogueInputRouter : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler input;

    private void Update()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;

        if (dialogueManager == null || input == null)
            return;

        if (!dialogueManager.IsActive)
            return;

        if (dialogueManager.State == DialogueState.Choosing)
        {
            if (input.PreviousPressed)
            {
                input.ConsumePrevious();
                dialogueManager.SelectPreviousChoice();
            }

            if (input.NextPressed)
            {
                input.ConsumeNext();
                dialogueManager.SelectNextChoice();
            }
        }

        if (!input.SubmitPressed)
            return;

        input.ConsumeSubmit();

        dialogueManager.Submit();
    }
}
