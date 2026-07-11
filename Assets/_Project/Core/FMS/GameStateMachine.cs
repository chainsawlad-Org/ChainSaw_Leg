using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;

public class GameStateMachine
{
    private readonly IPhaseFactory phaseFactory;
    private readonly GamePauseService gamePauseService;
    private readonly GameplayInputBlockService gameplayInputBlockService;
    private readonly Stack<OverlayPhase> overlayStack = new();

    private SceneGamePhase currentPhase;

    public event Action StateChanged;

    public GameStateMachine(
        IPhaseFactory phaseFactory,
        GamePauseService gamePauseService,
        GameplayInputBlockService gameplayInputBlockService)
    {
        this.phaseFactory = phaseFactory;
        this.gamePauseService = gamePauseService;
        this.gameplayInputBlockService = gameplayInputBlockService;
    }

    public async UniTask ReplaceMain<T>() where T : SceneGamePhase
    {
        await CloseAllOverlays();

        if (currentPhase != null)
        {
            await currentPhase.Exit();
        }

        currentPhase = phaseFactory.Get<T>();

        await currentPhase.Enter();
        NotifyStateChanged();
    }

    public async UniTask PushOverlay<T>() where T : OverlayPhase
    {
        var phase = phaseFactory.Get<T>();

        if (!CanPushOverlay(phase))
            return;

        overlayStack.Push(phase);
        ApplyOverlayState(phase, isActive: true);
        NotifyStateChanged();

        try
        {
            await phase.Enter();
        }
        catch
        {
            overlayStack.Pop();
            ApplyOverlayState(phase, isActive: false);
            NotifyStateChanged();
            throw;
        }
    }

    public async UniTask PushOverlay<T>(System.Action<T> configure) where T : OverlayPhase
    {
        var phase = phaseFactory.Get<T>();

        configure?.Invoke(phase);

        if (!CanPushOverlay(phase))
            return;

        overlayStack.Push(phase);
        ApplyOverlayState(phase, isActive: true);
        NotifyStateChanged();

        try
        {
            await phase.Enter();
        }
        catch
        {
            overlayStack.Pop();
            ApplyOverlayState(phase, isActive: false);
            NotifyStateChanged();
            throw;
        }
    }

    public async UniTask PopOverlay()
    {
        if (overlayStack.Count == 0)
            return;

        OverlayPhase phase = overlayStack.Pop();
        ApplyOverlayState(phase, isActive: false);
        NotifyStateChanged();

        await phase.Exit();
    }

    public async UniTask CloseAllOverlays()
    {
        while (overlayStack.Count > 0)
        {
            await PopOverlay();
        }
    }

    public bool HasOverlay => overlayStack.Count > 0;
    public SceneGamePhase CurrentMainPhase => currentPhase;
    public OverlayPhase CurrentOverlayPhase => overlayStack.Count > 0 ? overlayStack.Peek() : null;

    public bool HasOverlayOfType<T>() where T : OverlayPhase
    {
        foreach (OverlayPhase overlayPhase in overlayStack)
        {
            if (overlayPhase is T)
                return true;
        }

        return false;
    }

    public bool IsTopOverlay<T>() where T : OverlayPhase
    {
        return overlayStack.Count > 0 && overlayStack.Peek() is T;
    }

    private bool CanPushOverlay(OverlayPhase phase)
    {
        if (phase.CanStack)
            return true;

        foreach (OverlayPhase existingPhase in overlayStack)
        {
            if (existingPhase.GetType() == phase.GetType())
                return false;
        }

        return true;
    }

    private void ApplyOverlayState(OverlayPhase phase, bool isActive)
    {
        if (phase.BlockedInputChannels != InputBlockChannels.None)
        {
            if (isActive)
                gameplayInputBlockService.AcquireBlock(phase.BlockedInputChannels);
            else
                gameplayInputBlockService.ReleaseBlock(phase.BlockedInputChannels);
        }

        if (!phase.PausesGame)
            return;

        if (isActive)
            gamePauseService.AcquirePause();
        else
            gamePauseService.ReleasePause();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
