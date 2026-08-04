using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class PauseMenuExitCommandService : IDisposable
{
    private readonly GameStateMachine gameStateMachine;
    private readonly GamePauseService gamePauseService;
    private readonly IRuntimeErrorLogger runtimeErrorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    private int transitionState;
    private int disposeState;

    public PauseMenuExitCommandService(
        GameStateMachine gameStateMachine,
        GamePauseService gamePauseService,
        IRuntimeErrorLogger runtimeErrorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.gamePauseService = gamePauseService;
        this.runtimeErrorLogger = runtimeErrorLogger;
    }

    public bool IsTransitionInProgress => Volatile.Read(ref transitionState) != 0;

    public void RequestExitToMainMenu()
    {
        if (Volatile.Read(ref disposeState) != 0)
            return;

        if (Interlocked.CompareExchange(ref transitionState, 1, 0) != 0)
            return;

        ExitToMainMenuAsync(lifetimeCancellation.Token).Forget();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposeState, 1) != 0)
            return;

        lifetimeCancellation.Cancel();
        lifetimeCancellation.Dispose();
    }

    private async UniTask ExitToMainMenuAsync(CancellationToken cancellationToken)
    {
        try
        {
            await gameStateMachine.ReplaceMainAsync<MainMenuPhase>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            runtimeErrorLogger.LogException(exception, nameof(PauseMenuExitCommandService));
        }
        finally
        {
            gamePauseService.Reset();
            Interlocked.Exchange(ref transitionState, 0);
        }
    }
}
