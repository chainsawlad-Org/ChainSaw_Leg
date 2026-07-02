using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExplorationService : SceneService
{
    public override UniTask Initialize()
    {
        Debug.Log("ExplorationService Initialize");

        return UniTask.CompletedTask;
    }

    public override UniTask Dispose()
    {
        Debug.Log("ExplorationService Dispose");
        return UniTask.CompletedTask;
    }
}
