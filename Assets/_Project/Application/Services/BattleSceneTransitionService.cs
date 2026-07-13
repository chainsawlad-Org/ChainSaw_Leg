using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class BattleSceneTransitionService : IBattleSceneTransitionService, IDisposable
{
    private readonly GameStateMachine gameStateMachine;
    private readonly IRuntimeErrorLogger runtimeErrorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    private int transitionState;
    private int disposeState;

    public BattleSceneTransitionService(
        GameStateMachine gameStateMachine,
        IRuntimeErrorLogger runtimeErrorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.runtimeErrorLogger = runtimeErrorLogger;
    }

    public void RequestEnterBattle()
    {
        RequestTransition<BattlePhase>();
    }

    public void RequestReturnToExploration()
    {
        RequestTransition<ExplorationPhase>();
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
        if (Volatile.Read(ref disposeState) != 0)
            return;

        if (Interlocked.CompareExchange(ref transitionState, 1, 0) != 0)
            return;

        TransitionAsync<TPhase>(lifetimeCancellation.Token).Forget();
    }

    private async UniTask TransitionAsync<TPhase>(CancellationToken cancellationToken)
        where TPhase : SceneGamePhase
    {
        try
        {
            await gameStateMachine.ReplaceMainAsync<TPhase>(cancellationToken);
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
