using System;
using ChainSawLeg.Features.Dialogue;

namespace ChainSawLeg.Application
{
    public sealed class DialogueViewRegistry
    {
        private IDialogueView dialogueView;

        public void Register(IDialogueView view)
        {
            dialogueView = view
                           ?? throw new ArgumentNullException(nameof(view));
        }

        public void Unregister(IDialogueView view)
        {
            if (ReferenceEquals(dialogueView, view))
                dialogueView = null;
        }

        public IDialogueView Get()
        {
            return dialogueView
                   ?? throw new InvalidOperationException(
                       "Dialogue view has not been registered.");
        }
    }
}