using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogueService : SceneService
{
    public override UniTask Initialize()
    {
        Debug.Log("DialogueService Initialize");

        return UniTask.CompletedTask;
    }

    public override UniTask Dispose()
    {
        Debug.Log("DialogueService Dispose");

        return UniTask.CompletedTask;
    }
}