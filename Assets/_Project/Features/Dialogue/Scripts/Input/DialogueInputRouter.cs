using UnityEngine;

public class DialogueInputRouter : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler input;

    private void Update()
    {
        if (!DialogueManager.Instance.IsActive)
            return;

        if (!input.SubmitPressed)
            return;

        input.ConsumeSubmit();

        DialogueManager.Instance.Submit();
    }
}