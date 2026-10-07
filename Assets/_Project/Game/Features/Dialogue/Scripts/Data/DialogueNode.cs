using System.Collections.Generic;
using ChainSawLeg.Features.Dialogue;

public sealed class DialogueNode
{
    public string Id { get; }
    public DialogueNodeType Type { get; }
    public string Speaker { get; }
    public string Text { get; }
    
    public string NextNodeId { get; }
    
    public IReadOnlyList<DialogueChoice> Choices { get; }

    public DialogueNode(string id, DialogueNodeType type, string speaker, string text, string nextNodeId,
        IReadOnlyList<DialogueChoice> choices)
    {
        Id = id;
        Type = type;
        Speaker = speaker;
        Text = text;
        NextNodeId = nextNodeId;
        Choices = choices;
    }
}
