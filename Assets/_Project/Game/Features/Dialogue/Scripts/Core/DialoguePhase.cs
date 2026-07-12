using Cysharp.Threading.Tasks;

public class DialoguePhase : OverlayPhase
{
    private readonly DialogueService dialogueService;

    private DialogueRequest request;

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
        await dialogueService.Play(request);
    }

    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
