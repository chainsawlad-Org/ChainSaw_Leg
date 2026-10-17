using System.Collections.Generic;

namespace ChainSawLeg.Features.Dialogue
{
    public interface IDialogueDatabase
    {
        Dialogue GetDialogue(string dialogueId);

        bool TryGetDialogue(string dialogueId, out Dialogue dialogue);

        bool Contains(string dialogueId);

        IReadOnlyCollection<Dialogue> GetAll();
    }
}