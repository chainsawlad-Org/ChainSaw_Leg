
// Placement: Docs/Ru/02_ProjectStructure.md:192-202. Quote: "Содержит управление сценами."

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
