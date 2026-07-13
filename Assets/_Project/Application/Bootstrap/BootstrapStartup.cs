using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

public class BootstrapStartup : IInitializable, IDisposable
{
    private readonly IBootstrapRunner bootstrapRunner;
    private readonly IRuntimeErrorLogger errorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    public BootstrapStartup(
        IBootstrapRunner bootstrapRunner,
        IRuntimeErrorLogger errorLogger)
    {
        this.bootstrapRunner = bootstrapRunner;
        this.errorLogger = errorLogger;
    }

    public void Initialize()
    {
        RunAsync(lifetimeCancellation.Token).Forget();
    }

    public void Dispose()
    {
        lifetimeCancellation.Cancel();
        lifetimeCancellation.Dispose();
    }

    private async UniTask RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await bootstrapRunner.Run(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, nameof(BootstrapStartup));
        }
    }
}
