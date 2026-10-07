
namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueChoice
    {
        public string Id { get;  }
        public string Text { get; }
        public string NextNodeId { get; }

        public DialogueChoice(string id, string text, string nextNodeId)
        {
            Id = id;
            Text = text;
            NextNodeId = nextNodeId;
        }
    }
}
