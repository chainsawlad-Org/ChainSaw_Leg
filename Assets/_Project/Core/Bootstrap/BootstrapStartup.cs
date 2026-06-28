using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BootstrapStartup : IInitializable
{
    private readonly GameStateMachine gameStateMachine;
    private readonly ISceneLoader sceneLoader;

    public BootstrapStartup(GameStateMachine gameStateMachine, ISceneLoader sceneLoader)
    {
        this.gameStateMachine = gameStateMachine;
        this.sceneLoader = sceneLoader;
    }

    public void Initialize()

    {
        Debug.Log("Bootstrap Initialize");

        Run().Forget();
    }

    private async UniTask Run()

    {
        Debug.Log("Run");
        await sceneLoader.LoadAdditive(SceneNames.Persistent);
        Debug.Log("Persistent loaded");
        await gameStateMachine.ReplaceMain<MainMenuPhase>();
        Debug.Log("MainMenu entered");
    }
}
