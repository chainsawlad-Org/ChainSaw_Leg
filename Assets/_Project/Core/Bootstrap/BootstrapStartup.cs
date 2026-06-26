using Cysharp.Threading.Tasks;
using NUnit.Framework;
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
        Start().Forget();
    }

    private async UniTask Start()
    {
        await sceneLoader.LoadAdditive("SC_Persistent");

        await gameStateMachine.Enter<MainMenuPhase>();
    }
}
