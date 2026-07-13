using Cysharp.Threading.Tasks;

public abstract class SceneService : ISceneService
{
    public virtual UniTask Initialize()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask Dispose()
    {
        return UniTask.CompletedTask;
    }
}
