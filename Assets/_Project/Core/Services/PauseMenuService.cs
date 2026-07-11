using System;
using Cysharp.Threading.Tasks;

public class PauseMenuService
{
    private readonly GameStateMachine gameStateMachine;
    private readonly ISceneLoader sceneLoader;

    public PauseMenuService(
        GameStateMachine gameStateMachine,
        ISceneLoader sceneLoader)
    {
        this.gameStateMachine = gameStateMachine;
        this.sceneLoader = sceneLoader;
    }

    public event Action PauseRequested;

    public void RequestPause()
    {
        if (!CanHandlePauseRequest())
            return;

        PauseRequested?.Invoke();
        TogglePauseMenu().Forget();
    }

    private bool CanHandlePauseRequest()
    {
        if (gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
            return true;

        if (gameStateMachine.IsTopOverlay<PauseMenuPhase>())
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

    private async UniTaskVoid TogglePauseMenu()
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

        await gameStateMachine.PushOverlay<PauseMenuPhase>();
    }
}
