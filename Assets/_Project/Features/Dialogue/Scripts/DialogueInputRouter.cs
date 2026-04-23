using UnityEngine;

public class DialogueInputRouter : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler input;

    private void Awake()
    {
        if (input == null)
            input = FindAnyObjectByType<PlayerInputHandler>();
    }

    private void Update()
    {
        if (!DialogueManager.Instance.IsActive) return;

        if (!input.SubmitPressed) return;

        input.ConsumeSubmit();

        if (!DialogueManager.Instance.HasChoices)
        {
            DialogueManager.Instance.Next();
        }
    }
}
