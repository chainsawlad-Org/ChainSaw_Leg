using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Zenject;

public class BootstrapStartup : IInitializable, IDisposable
{
    private readonly GameStateMachine gameStateMachine;
    private readonly ISceneLoader sceneLoader;
    private readonly IRuntimeErrorLogger errorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    public BootstrapStartup(
        GameStateMachine gameStateMachine,
        ISceneLoader sceneLoader,
        IRuntimeErrorLogger errorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.sceneLoader = sceneLoader;
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
            await sceneLoader.LoadAdditiveAsync(SceneNames.Persistent, cancellationToken);

#if UNITY_EDITOR
            string activeScene = SceneManager.GetActiveScene().name;

            if (activeScene == SceneNames.World)
            {
                sceneLoader.SetCurrentScene(SceneNames.World);
                await gameStateMachine.ReplaceMainAsync<ExplorationPhase>(cancellationToken);
                return;
            }

            if (activeScene == SceneNames.Battle)
            {
                sceneLoader.SetCurrentScene(SceneNames.Battle);
                return;
            }
#endif

            await gameStateMachine.ReplaceMainAsync<MainMenuPhase>(cancellationToken);
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
