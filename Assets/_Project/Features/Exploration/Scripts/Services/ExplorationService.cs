using Cysharp.Threading.Tasks;

public class ExplorationService : SceneService
{
    public override UniTask Initialize()
    {
        return UniTask.CompletedTask;
    }

    public override UniTask Dispose()
    {
        return UniTask.CompletedTask;
    }
}
