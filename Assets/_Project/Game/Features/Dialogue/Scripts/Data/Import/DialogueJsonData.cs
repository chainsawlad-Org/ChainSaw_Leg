using System.Collections.Generic;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueJsonData
    {
        public List<DialogueJsonEntry> Dialogues { get; set; }
        public List<DialogueNodeJsonEntry> Nodes { get; set; }
        public List<DialogueChoiceJsonEntry> Choices { get; set; }
    }
}
