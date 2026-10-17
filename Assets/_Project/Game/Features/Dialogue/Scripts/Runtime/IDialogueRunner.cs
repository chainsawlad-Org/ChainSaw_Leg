using System;

namespace ChainSawLeg.Features.Dialogue
{
    public interface IDialogueRunner
    {
        DialogueRuntimeState State { get; }
        
        Dialogue CurrentDialogue { get; }
        
        DialogueNode CurrentNode { get; }

        event Action<Dialogue> DialogueStarted;

        event Action<DialogueNode> NodeChanged;

        event Action<Dialogue> DialogueEnded;

        void Start(string dialogueId);

        void Continue();

        void SelectChoice(string choiceId);

        void Stop();
    }
}
