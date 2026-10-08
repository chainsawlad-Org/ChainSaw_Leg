using System.Collections.Generic;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class Dialogue
    {
        private readonly IReadOnlyDictionary<string, DialogueNode> nodes;
        
        public string Id { get; }
        public string Title { get; }
        public string StartNodeId { get; }

        public Dialogue(string id, string title, string startNodeId, IReadOnlyDictionary<string, DialogueNode> nodes)
        {
            Id = id;
            Title = title;
            StartNodeId = startNodeId;
            this.nodes = nodes;
        }

        public bool TryGetNode(string nodeId, out DialogueNode node)
        {
            return nodes.TryGetValue(nodeId, out node);
        }
    }
}