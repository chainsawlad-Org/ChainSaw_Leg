using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class DialogueService
{
    private readonly IRuntimeErrorLogger errorLogger;
    private readonly DialogueRuntimeRegistry runtimeRegistry;

    public DialogueService(
        IRuntimeErrorLogger errorLogger,
        DialogueRuntimeRegistry runtimeRegistry)
    {
        this.errorLogger = errorLogger;
        this.runtimeRegistry = runtimeRegistry;
    }

    public async UniTask Play(DialogueRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        cancellationToken.ThrowIfCancellationRequested();
        IDialogueRuntime dialogueManager = runtimeRegistry.Current;

        if (dialogueManager == null)
        {
            var exception = new InvalidOperationException("DialogueManager is not active in the current scene.");
            errorLogger.LogException(exception, nameof(DialogueService));
            throw exception;
        }

        var completion = new UniTaskCompletionSource();

        void Finished()
        {
            completion.TrySetResult();
        }

        void ManagerUnregistered(IDialogueRuntime manager)
        {
            if (manager == dialogueManager)
                completion.TrySetCanceled(cancellationToken);
        }

        dialogueManager.DialogueFinished += Finished;
        runtimeRegistry.ManagerUnregistered += ManagerUnregistered;
        using CancellationTokenRegistration registration = cancellationToken.Register(
            () => completion.TrySetCanceled(cancellationToken));

        try
        {
            dialogueManager.StartDialogue(
                request.Events,
                request.Type,
                request.Speaker);

            await completion.Task;
        }
        finally
        {
            dialogueManager.DialogueFinished -= Finished;
            runtimeRegistry.ManagerUnregistered -= ManagerUnregistered;
        }
    }
}
