using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class DialoguePhase : OverlayPhase
{
    private readonly DialogueService dialogueService;

    private DialogueRequest request;
    private CancellationTokenSource activeCancellation;

    public override InputBlockChannels BlockedInputChannels =>
        InputBlockChannels.Move | InputBlockChannels.Dash | InputBlockChannels.Interact;

    public DialoguePhase(DialogueService dialogueService)
    {
        this.dialogueService = dialogueService;
    }

    public void Configure(DialogueRequest request)
    {
        this.request = request;
    }

    public override async UniTask Enter()
    {
        if (request == null)
            throw new InvalidOperationException("Dialogue phase request is not configured.");

        CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            request.CancellationToken);
        activeCancellation = cancellation;

        try
        {
            await dialogueService.Play(request, cancellation.Token);
        }
        finally
        {
            if (ReferenceEquals(activeCancellation, cancellation))
                activeCancellation = null;

            cancellation.Dispose();
        }
    }

    public override UniTask Exit()
    {
        activeCancellation?.Cancel();
        request = null;
        return UniTask.CompletedTask;
    }
}
