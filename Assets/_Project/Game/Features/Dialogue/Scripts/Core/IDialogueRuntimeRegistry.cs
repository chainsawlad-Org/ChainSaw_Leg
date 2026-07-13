public interface IDialogueRuntimeRegistry
{
    void Register(IDialogueRuntime dialogueRuntime);
    void Unregister(IDialogueRuntime dialogueRuntime);
}
