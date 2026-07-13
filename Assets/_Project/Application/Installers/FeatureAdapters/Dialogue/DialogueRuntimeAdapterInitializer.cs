using Zenject;

public sealed class DialogueRuntimeAdapterInitializer : IInitializable
{
    private readonly DialogueManager dialogueManager;
    private readonly IDialogueRuntimeRegistry runtimeRegistry;

    public DialogueRuntimeAdapterInitializer(
        DialogueManager dialogueManager,
        IDialogueRuntimeRegistry runtimeRegistry)
    {
        this.dialogueManager = dialogueManager;
        this.runtimeRegistry = runtimeRegistry;
    }

    public void Initialize()
    {
        dialogueManager.Initialize(runtimeRegistry);
    }
}
