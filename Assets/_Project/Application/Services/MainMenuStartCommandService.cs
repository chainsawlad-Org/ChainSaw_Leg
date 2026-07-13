using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class MainMenuStartCommandService : IDisposable
{
    private readonly GameStateMachine gameStateMachine;
    private readonly BattleSessionService battleSessionService;
    private readonly IRuntimeErrorLogger runtimeErrorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    private int transitionState;
    private int disposeState;

    public MainMenuStartCommandService(
        GameStateMachine gameStateMachine,
        BattleSessionService battleSessionService,
        IRuntimeErrorLogger runtimeErrorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.battleSessionService = battleSessionService;
        this.runtimeErrorLogger = runtimeErrorLogger;
    }

    public bool IsTransitionInProgress => Volatile.Read(ref transitionState) != 0;

    public void RequestStartGame()
    {
        if (Volatile.Read(ref disposeState) != 0)
            return;

        if (Interlocked.CompareExchange(ref transitionState, 1, 0) != 0)
            return;

        battleSessionService.Reset();
        StartGameAsync(lifetimeCancellation.Token).Forget();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposeState, 1) != 0)
            return;

        lifetimeCancellation.Cancel();
        lifetimeCancellation.Dispose();
    }

    private async UniTask StartGameAsync(CancellationToken cancellationToken)
    {
        try
        {
            await gameStateMachine.ReplaceMainAsync<ExplorationPhase>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            runtimeErrorLogger.LogException(exception, nameof(MainMenuStartCommandService));
        }
        finally
        {
            Interlocked.Exchange(ref transitionState, 0);
        }
    }
}
