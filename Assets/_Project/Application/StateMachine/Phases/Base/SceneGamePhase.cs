using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class SceneGamePhase : GamePhase
{
    protected readonly ISceneLoader sceneLoader;

    protected SceneGamePhase(ISceneLoader sceneLoader)
    {
        this.sceneLoader = sceneLoader;
    }

    protected abstract string SceneName { get; }
    public virtual bool AllowsGameplayInput => true;

    public override UniTask Enter()
    {
        return EnterAsync(CancellationToken.None);
    }

    public async UniTask EnterAsync(CancellationToken cancellationToken)
    {
        await sceneLoader.SwitchToAsync(SceneName, cancellationToken);
    }

    public async UniTask ReloadAsync(CancellationToken cancellationToken)
    {
        await sceneLoader.ReloadAsync(SceneName, cancellationToken);
    }

    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
