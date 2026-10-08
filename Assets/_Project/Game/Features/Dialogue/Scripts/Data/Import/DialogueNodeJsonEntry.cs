using UnityEngine;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueNodeJsonEntry
    {
        public string node_id { get; set; }
        public string dialogue_id { get; set; }
        public string type { get; set; }
        public string speaker { get; set; }
        public string text { get; set; }
        public string next_node_id { get; set; }
    }
}
