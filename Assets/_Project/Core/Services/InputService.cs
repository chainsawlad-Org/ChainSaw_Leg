using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InputService : IInitializable, IDisposable
{
    private readonly PauseMenuService pauseMenuService;
    private readonly PlayerInputActions input;

    public InputService(PauseMenuService pauseMenuService)
    {
        this.pauseMenuService = pauseMenuService;
        input = new PlayerInputActions();
    }

    public Vector2 MoveInput { get; private set; }
    public bool DashPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool SubmitPressed { get; private set; }

    public void Initialize()
    {
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
        input.Player.Dash.performed += OnDash;
        input.Player.Interact.performed += OnInteract;
        input.Player.Submit.performed += OnSubmit;
        input.Player.Pause.performed += OnPause;

        input.Player.Enable();
    }

    public void Dispose()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;
        input.Player.Dash.performed -= OnDash;
        input.Player.Interact.performed -= OnInteract;
        input.Player.Submit.performed -= OnSubmit;
        input.Player.Pause.performed -= OnPause;

        input.Player.Disable();
        input.Dispose();
    }

    public void ConsumeDash()
    {
        DashPressed = false;
    }

    public void ConsumeInteract()
    {
        InteractPressed = false;
    }

    public void ConsumeSubmit()
    {
        SubmitPressed = false;
    }

    public void ResetTransientInput()
    {
        MoveInput = Vector2.zero;
        DashPressed = false;
        InteractPressed = false;
        SubmitPressed = false;
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

    private void OnSubmit(InputAction.CallbackContext context)
    {
        SubmitPressed = true;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        pauseMenuService.RequestPause();
    }
}
