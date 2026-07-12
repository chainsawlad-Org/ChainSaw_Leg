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

    public async UniTask Play(DialogueRequest request)
    {
        var completion = new UniTaskCompletionSource();

        void Finished()
        {
            DialogueManager.Instance.DialogueFinished -= Finished;
            completion.TrySetResult();
        }

        DialogueManager.Instance.DialogueFinished += Finished;

        DialogueManager.Instance.StartDialogue(
            request.Events,
            request.Type,
            request.Speaker);

        await completion.Task;
    }
}