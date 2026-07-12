using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class GameStateMachine
{
    private readonly IPhaseFactory phaseFactory;
    private readonly GamePauseService gamePauseService;
    private readonly GameplayInputBlockService gameplayInputBlockService;
    private readonly Stack<OverlayPhase> overlayStack = new();

    private SceneGamePhase currentPhase;
    private bool isMainGameplayInputBlocked;
    private int mainTransitionState;

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

    public UniTask ReplaceMain<T>() where T : SceneGamePhase
    {
        return ReplaceMainAsync<T>(CancellationToken.None);
    }

    public UniTask ReplaceMainAsync<T>(CancellationToken cancellationToken) where T : SceneGamePhase
    {
        return TransitionMainAsync(typeof(T), null, cancellationToken, forceReload: false);
    }

    public UniTask ReplaceMainAsync(Type phaseType, CancellationToken cancellationToken)
    {
        return TransitionMainAsync(phaseType, null, cancellationToken, forceReload: false);
    }

    public UniTask ReloadMainAsync<T>(CancellationToken cancellationToken) where T : SceneGamePhase
    {
        return TransitionMainAsync(typeof(T), null, cancellationToken, forceReload: true);
    }

    public UniTask ReloadMainAsync<T>(Action<T> configure, CancellationToken cancellationToken)
        where T : SceneGamePhase
    {
        return TransitionMainAsync(typeof(T), phase => configure?.Invoke((T)phase), cancellationToken, forceReload: true);
    }

    private async UniTask TransitionMainAsync(
        Type phaseType,
        Action<SceneGamePhase> configure,
        CancellationToken cancellationToken,
        bool forceReload)
    {
        if (phaseType == null)
            throw new ArgumentNullException(nameof(phaseType));

        if (!typeof(SceneGamePhase).IsAssignableFrom(phaseType))
            throw new ArgumentException($"Type {phaseType.FullName} is not a scene game phase.", nameof(phaseType));

        if (Interlocked.CompareExchange(ref mainTransitionState, 1, 0) != 0)
            return;

        SceneGamePhase previousPhase = currentPhase;
        SceneGamePhase nextPhase = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CloseAllOverlaysAsync(cancellationToken);
            gamePauseService.Reset();

            if (previousPhase != null)
                await previousPhase.Exit();

            cancellationToken.ThrowIfCancellationRequested();
            nextPhase = (SceneGamePhase)phaseFactory.Get(phaseType);
            configure?.Invoke(nextPhase);

            if (!nextPhase.AllowsGameplayInput)
                ApplyMainPhaseInputState(nextPhase);

            if (forceReload)
                await nextPhase.ReloadAsync(cancellationToken);
            else
                await nextPhase.EnterAsync(cancellationToken);
            currentPhase = nextPhase;
            ApplyMainPhaseInputState(nextPhase);
            NotifyStateChanged();
        }
        catch
        {
            ApplyMainPhaseInputState(previousPhase);
            throw;
        }
        finally
        {
            Interlocked.Exchange(ref mainTransitionState, 0);
        }
    }

    public UniTask CloseAllOverlays()
    {
        return CloseAllOverlaysAsync(CancellationToken.None);
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
            RollbackFailedOverlay(phase);
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
            RollbackFailedOverlay(phase);
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

    public async UniTask CloseAllOverlaysAsync(CancellationToken cancellationToken)
    {
        while (overlayStack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
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

    private void RollbackFailedOverlay(OverlayPhase phase)
    {
        if (overlayStack.Count == 0 || !ReferenceEquals(overlayStack.Peek(), phase))
            return;

        overlayStack.Pop();
        ApplyOverlayState(phase, isActive: false);
        NotifyStateChanged();
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

    private void ApplyMainPhaseInputState(SceneGamePhase phase)
    {
        bool shouldBlockGameplayInput = phase != null && !phase.AllowsGameplayInput;

        if (shouldBlockGameplayInput == isMainGameplayInputBlocked)
            return;

        if (shouldBlockGameplayInput)
            gameplayInputBlockService.AcquireBlock(InputBlockChannels.Gameplay);
        else
            gameplayInputBlockService.ReleaseBlock(InputBlockChannels.Gameplay);

        isMainGameplayInputBlocked = shouldBlockGameplayInput;
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
