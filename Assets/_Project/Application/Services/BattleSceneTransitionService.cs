using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class BattleSceneTransitionService : IBattleSceneTransitionService, IDisposable
{
    private readonly GameStateMachine gameStateMachine;
    private readonly BattleSessionService battleSessionService;
    private readonly IRuntimeErrorLogger runtimeErrorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    private int transitionState;
    private int disposeState;

    public BattleSceneTransitionService(
        GameStateMachine gameStateMachine,
        BattleSessionService battleSessionService,
        IRuntimeErrorLogger runtimeErrorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.battleSessionService = battleSessionService;
        this.runtimeErrorLogger = runtimeErrorLogger;
    }

    public void RequestEnterBattle()
    {
        RequestTransition<BattlePhase>();
    }

    public void RequestReturnToExploration()
    {
        string returnSceneName = battleSessionService.TryConsumeReturnSceneName(out string sceneName)
            ? sceneName
            : SceneNames.World;

        RequestTransition(cancellationToken =>
            gameStateMachine.ReloadMainAsync<ExplorationPhase>(
                phase => phase.SetTargetScene(returnSceneName),
                cancellationToken));
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposeState, 1) != 0)
            return;

        lifetimeCancellation.Cancel();
        lifetimeCancellation.Dispose();
    }

    private void RequestTransition<TPhase>() where TPhase : SceneGamePhase
    {
        RequestTransition(cancellationToken =>
            gameStateMachine.ReplaceMainAsync<TPhase>(cancellationToken));
    }

    private void RequestTransition(Func<CancellationToken, UniTask> transition)
    {
        if (Volatile.Read(ref disposeState) != 0)
            return;

        if (Interlocked.CompareExchange(ref transitionState, 1, 0) != 0)
            return;

        TransitionAsync(transition, lifetimeCancellation.Token).Forget();
    }

    private async UniTask TransitionAsync(
        Func<CancellationToken, UniTask> transition,
        CancellationToken cancellationToken)
    {
        try
        {
            await transition(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            runtimeErrorLogger.LogException(exception, nameof(BattleSceneTransitionService));
        }
        finally
        {
            Interlocked.Exchange(ref transitionState, 0);
        }
    }
}
