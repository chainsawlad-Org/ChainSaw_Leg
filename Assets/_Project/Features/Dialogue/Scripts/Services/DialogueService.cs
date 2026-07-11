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
        DialogueManager dialogueManager = DialogueManager.Instance;

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueService.Play called without an active DialogueManager in scene.");
            return;
        }

        var completion = new UniTaskCompletionSource();

        void Finished()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.DialogueFinished -= Finished;

            completion.TrySetResult();
        }

        dialogueManager.DialogueFinished += Finished;

        dialogueManager.StartDialogue(
            request.Events,
            request.Type,
            request.Speaker);

        await completion.Task;
    }
}
