using Cysharp.Threading.Tasks;

public class DialogueService : SceneService
{
    private readonly IRuntimeErrorLogger errorLogger;

    public DialogueService(IRuntimeErrorLogger errorLogger)
    {
        this.errorLogger = errorLogger;
    }

    public override UniTask Initialize()
    {
        return UniTask.CompletedTask;
    }

    public override UniTask Dispose()
    {
        return UniTask.CompletedTask;
    }

    public async UniTask Play(DialogueRequest request)
    {
        DialogueManager dialogueManager = DialogueManager.Instance;

        if (dialogueManager == null)
        {
            errorLogger.LogException(
                new System.InvalidOperationException("DialogueManager is not active in the current scene."),
                nameof(DialogueService));
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
