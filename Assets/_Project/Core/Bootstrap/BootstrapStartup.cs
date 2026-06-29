using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class BootstrapStartup : IInitializable
{
    private readonly GameStateMachine gameStateMachine;
    private readonly ISceneLoader sceneLoader;

    public BootstrapStartup(
        GameStateMachine gameStateMachine,
        ISceneLoader sceneLoader)
    {
        this.gameStateMachine = gameStateMachine;
        this.sceneLoader = sceneLoader;
    }

    public void Initialize()
    {
        Debug.Log("========== Bootstrap Initialize ==========");

        Run().Forget();
    }

    private async UniTask Run()
    {
        Debug.Log("Bootstrap: Run started");

        Debug.Log($"Bootstrap: Loading {SceneNames.Persistent}");
        await sceneLoader.LoadAdditive(SceneNames.Persistent);
        Debug.Log("Bootstrap: Persistent loaded");

#if UNITY_EDITOR

        string activeScene = SceneManager.GetActiveScene().name;

        Debug.Log($"Bootstrap: Active Scene = '{activeScene}'");
        Debug.Log($"Bootstrap: World Scene  = '{SceneNames.World}'");
        Debug.Log($"Bootstrap: Equals = {activeScene == SceneNames.World}");

        Debug.Log($"Bootstrap: Loaded scenes = {SceneManager.sceneCount}");

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            Debug.Log(
                $"Bootstrap: [{i}] " +
                $"Name={scene.name}, " +
                $"Loaded={scene.isLoaded}, " +
                $"Valid={scene.IsValid()}"
            );
        }

        if (activeScene == SceneNames.World)
        {
            Debug.Log("Bootstrap: >>> ENTER EXPLORATION <<<");

            (sceneLoader as SceneLoader)?.SetCurrentScene(SceneNames.World);
            sceneLoader.SetCurrentScene(SceneNames.World);

            await gameStateMachine.ReplaceMain<ExplorationPhase>();

            Debug.Log("Bootstrap: ExplorationPhase finished Enter()");

            return;
        }

        if (activeScene == SceneNames.Battle)
        {
            Debug.Log("Bootstrap: >>> ENTER BATTLE <<<");
            sceneLoader.SetCurrentScene(SceneNames.Battle);
            return;
        }

#endif

        Debug.Log("Bootstrap: >>> ENTER MAIN MENU <<<");

        await gameStateMachine.ReplaceMain<MainMenuPhase>();

        Debug.Log("Bootstrap: MainMenuPhase finished Enter()");
    }
}