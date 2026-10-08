
namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueChoiceJsonEntry
    {
        public string choice_id { get; set; }
        public string dialogue_id { get; set; }
        public string node_id { get; set; }
        public string text { get; set; }
        public string next_node_id { get; set; }
    }
}
