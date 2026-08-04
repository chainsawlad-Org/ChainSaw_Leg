using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InputService : IGameInputService, IInitializable, IDisposable
{
    private readonly IPauseRequestHandler pauseRequestHandler;
    private readonly IGameplayInputBlockService gameplayInputBlockService;
    private readonly PlayerInputActions input;
    private readonly InputActionMap systemInputMap;
    private readonly InputAction pauseAction;

    public InputService(
        IPauseRequestHandler pauseRequestHandler,
        IGameplayInputBlockService gameplayInputBlockService)
    {
        this.pauseRequestHandler = pauseRequestHandler;
        this.gameplayInputBlockService = gameplayInputBlockService;
        input = new PlayerInputActions();
        systemInputMap = input.asset.FindActionMap("System", throwIfNotFound: true);
        pauseAction = systemInputMap.FindAction("Pause", throwIfNotFound: true);
    }

    public bool IsReady => isInitialized && !isDisposed;
    public bool IsGameplayInputEnabled => IsReady && input.Player.enabled;
    public Vector2 MoveInput => !IsGameplayInputEnabled ||
        gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Move)
        ? Vector2.zero
        : input.Player.Move.ReadValue<Vector2>();

    public bool DashPressed => !gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Dash) && dashPressed;
    public bool InteractPressed => !gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Interact) && interactPressed;
    public bool SubmitPressed => !gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Submit) && submitPressed;
    public bool UiSubmitPressed => uiSubmitPressed;
    public bool PreviousPressed => previousPressed;
    public bool NextPressed => nextPressed;

    private bool dashPressed;
    private bool interactPressed;
    private bool submitPressed;
    private bool uiSubmitPressed;
    private bool previousPressed;
    private bool nextPressed;
    private bool isInitialized;
    private bool isDisposed;

    public void Initialize()
    {
        if (isInitialized || isDisposed)
            return;

        gameplayInputBlockService.BlockStateChanged += OnGameplayBlockStateChanged;
        input.Player.Dash.performed += OnDash;
        input.Player.Interact.performed += OnInteract;
        input.Player.Submit.performed += OnSubmit;
        input.Player.Previous.performed += OnPrevious;
        input.Player.Next.performed += OnNext;
        pauseAction.performed += OnPause;
        RegisterLifecycleCallbacks();

        systemInputMap.Enable();
        input.Player.Enable();
        ApplyGameplayInputState();
        isInitialized = true;
    }

    public void Dispose()
    {
        Cleanup();
    }

    public void ConsumeDash()
    {
        dashPressed = false;
    }

    public void ConsumeInteract()
    {
        interactPressed = false;
    }

    public void ConsumeSubmit()
    {
        submitPressed = false;
        uiSubmitPressed = false;
    }

    public void ConsumePrevious()
    {
        previousPressed = false;
    }

    public void ConsumeNext()
    {
        nextPressed = false;
    }

    public void ResetTransientInput()
    {
        dashPressed = false;
        interactPressed = false;
        submitPressed = false;
        uiSubmitPressed = false;
        previousPressed = false;
        nextPressed = false;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Dash))
            return;

        dashPressed = true;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Interact))
            return;

        interactPressed = true;
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        uiSubmitPressed = true;

        if (gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Submit))
            return;

        submitPressed = true;
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        previousPressed = true;
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        nextPressed = true;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        pauseRequestHandler.RequestPause();
    }

    private void OnGameplayBlockStateChanged()
    {
        ApplyGameplayInputState();
    }

    private void ApplyGameplayInputState()
    {
        if (gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Move))
        {
            previousPressed = false;
            nextPressed = false;
        }

        if (gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Dash))
            dashPressed = false;

        if (gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Interact))
            interactPressed = false;

        if (gameplayInputBlockService.IsChannelBlocked(InputBlockChannels.Submit))
        {
            submitPressed = false;
            uiSubmitPressed = false;
        }
    }

    private void Cleanup()
    {
        if (isDisposed)
            return;

        UnregisterLifecycleCallbacks();

        if (isInitialized)
        {
            gameplayInputBlockService.BlockStateChanged -= OnGameplayBlockStateChanged;
            input.Player.Dash.performed -= OnDash;
            input.Player.Interact.performed -= OnInteract;
            input.Player.Submit.performed -= OnSubmit;
            input.Player.Previous.performed -= OnPrevious;
            input.Player.Next.performed -= OnNext;
            pauseAction.performed -= OnPause;
        }

        ResetTransientInput();

        if (input.Player.enabled)
            input.Player.Disable();

        if (systemInputMap.enabled)
            systemInputMap.Disable();

        input.Dispose();
        isDisposed = true;
        isInitialized = false;
    }

    private void OnDomainUnload(object sender, EventArgs args)
    {
        Cleanup();
    }

    private void OnProcessExit(object sender, EventArgs args)
    {
        Cleanup();
    }

#if UNITY_EDITOR
    private void OnBeforeAssemblyReload()
    {
        Cleanup();
    }
#endif

    private void RegisterLifecycleCallbacks()
    {
        AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

#if UNITY_EDITOR
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
#endif
    }

    private void UnregisterLifecycleCallbacks()
    {
        AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;

#if UNITY_EDITOR
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
#endif
    }
}
