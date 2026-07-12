using System;

public sealed class DialogueRuntimeRegistry
{
    public event Action<DialogueManager> ManagerUnregistered;

    public DialogueManager Current { get; private set; }

    public void Register(DialogueManager manager)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));

        if (Current != null && Current != manager)
            throw new InvalidOperationException("A dialogue manager is already registered.");

        Current = manager;
    }

    public void Unregister(DialogueManager manager)
    {
        if (Current != manager)
            return;

        Current = null;
        ManagerUnregistered?.Invoke(manager);
    }
}
