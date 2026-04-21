using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions input;

    public bool DashPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;

        input.Player.Dash.performed += OnDash;
        input.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;

        input.Player.Dash.performed -= OnDash;
        input.Player.Interact.performed -= OnInteract;

        input.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        DashPressed = true;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        InteractPressed = true;
    }

    public void ConsumeDash()
    {
        DashPressed = false;
    }

    public void ConsumeInteract()
    {
        InteractPressed = false;
    }
}