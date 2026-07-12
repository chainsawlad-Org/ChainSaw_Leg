using Cysharp.Threading.Tasks;

public abstract class SceneGamePhase : GamePhase
{
    protected readonly ISceneLoader sceneLoader;

    protected SceneGamePhase(ISceneLoader sceneLoader)
    {
        this.sceneLoader = sceneLoader;
    }

    protected abstract string SceneName { get; }

    public override async UniTask Enter()
    {
        await sceneLoader.SwitchTo(SceneName);
    }

    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
