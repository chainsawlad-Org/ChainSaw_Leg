using System;

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
        if (!CanOpenPause())
            return;

        PauseRequested?.Invoke();
    }

    private bool CanOpenPause()
    {
        SceneGamePhase currentMainPhase = gameStateMachine.CurrentMainPhase;

        if (currentMainPhase == null)
        {
            string loadedGameplayScene = sceneLoader.LoadedGameplayScene;

            return !string.IsNullOrEmpty(loadedGameplayScene)
                && loadedGameplayScene != SceneNames.MainMenu;
        }

        return currentMainPhase is not MainMenuPhase;
    }
}
