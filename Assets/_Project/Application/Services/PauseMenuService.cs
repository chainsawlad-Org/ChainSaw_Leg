using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PauseMenuService : IPauseRequestHandler
{
    private readonly GameStateMachine gameStateMachine;
    private readonly ISceneLoader sceneLoader;
    private readonly IRuntimeErrorLogger errorLogger;

    private int toggleState;

    public PauseMenuService(
        GameStateMachine gameStateMachine,
        ISceneLoader sceneLoader,
        IRuntimeErrorLogger errorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.sceneLoader = sceneLoader;
        this.errorLogger = errorLogger;
    }

    public event Action PauseRequested;

    public void RequestPause()
    {
        if (!CanHandlePauseRequest())
            return;

        if (Interlocked.CompareExchange(ref toggleState, 1, 0) != 0)
            return;

        HandlePauseRequestAsync().Forget();
    }

    private bool CanHandlePauseRequest()
    {
        if (gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
            return true;

        if (gameStateMachine.IsTopOverlay<PauseMenuPhase>())
            return true;

        if (gameStateMachine.IsTopOverlay<CheckpointSavePhase>())
            return true;

        SceneGamePhase currentMainPhase = gameStateMachine.CurrentMainPhase;

        if (currentMainPhase == null)
        {
            string loadedGameplayScene = sceneLoader.LoadedGameplayScene;

            return !string.IsNullOrEmpty(loadedGameplayScene)
                && loadedGameplayScene != SceneNames.MainMenu;
        }

        return currentMainPhase is not MainMenuPhase;
    }

    private async UniTask HandlePauseRequestAsync()
    {
        try
        {
            PauseRequested?.Invoke();
            await TogglePauseMenuAsync();
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, nameof(PauseMenuService));
        }
        finally
        {
            Interlocked.Exchange(ref toggleState, 0);
        }
    }

    private async UniTask TogglePauseMenuAsync()
    {
        if (gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
        {
            await gameStateMachine.PopOverlay();
            return;
        }

        if (gameStateMachine.IsTopOverlay<PauseMenuPhase>())
        {
            await gameStateMachine.PopOverlay();
            return;
        }

        if (gameStateMachine.IsTopOverlay<CheckpointSavePhase>())
        {
            await gameStateMachine.PopOverlay();
            return;
        }

        await gameStateMachine.PushOverlay<PauseMenuPhase>();
    }
}
