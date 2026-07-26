using System;

public sealed class DialogueRuntimeRegistry : IDialogueRuntimeRegistry
{
    public event Action<IDialogueRuntime> ManagerUnregistered;

    public IDialogueRuntime Current { get; private set; }

    public void Register(IDialogueRuntime manager)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));

        if (Current != null && Current != manager)
            throw new InvalidOperationException("A dialogue manager is already registered.");

        Current = manager;
    }

    public void Unregister(IDialogueRuntime manager)
    {
        if (Current != manager)
            return;

        Current = null;
        ManagerUnregistered?.Invoke(manager);
    }
}
