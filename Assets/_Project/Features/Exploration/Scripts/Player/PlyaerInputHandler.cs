using UnityEngine;
using Zenject;

public class PlayerInputHandler : MonoBehaviour
{
    private InputService inputService;

    public bool DashPressed => inputService != null && inputService.DashPressed;
    public bool InteractPressed => inputService != null && inputService.InteractPressed;
    public Vector2 MoveInput => inputService != null ? inputService.MoveInput : Vector2.zero;
    public bool SubmitPressed => inputService != null && inputService.SubmitPressed;
    public bool PreviousPressed => inputService != null && inputService.PreviousPressed;
    public bool NextPressed => inputService != null && inputService.NextPressed;

    [Inject]
    public void Construct(InputService inputService)
    {
        this.inputService = inputService;
    }

    public void ConsumeDash()
    {
        inputService?.ConsumeDash();
    }

    public void ConsumeInteract()
    {
        inputService?.ConsumeInteract();
    }

    public void ConsumeSubmit()
    {
        inputService?.ConsumeSubmit();
    }

    public void ConsumePrevious()
    {
        inputService?.ConsumePrevious();
    }

    public void ConsumeNext()
    {
        inputService?.ConsumeNext();
    }

    private void OnDisable()
    {
        inputService?.ResetTransientInput();
    }
}
