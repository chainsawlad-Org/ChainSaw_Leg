using Cysharp.Threading.Tasks;
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
        Run().Forget();
    }

    private async UniTask Run()
    {
        await sceneLoader.LoadAdditive(SceneNames.Persistent);
        await gameStateMachine.Enter<MainMenuPhase>();
    }
}
