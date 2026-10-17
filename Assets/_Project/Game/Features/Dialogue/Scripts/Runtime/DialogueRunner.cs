using System;
using System.Linq;

namespace ChainSawLeg.Features.Dialogue
{
    public class DialogueRunner : IDialogueRunner
    {
        private readonly IDialogueDatabase dialogueDatabase;
        
        public DialogueRuntimeState State { get; private set; } = DialogueRuntimeState.Idle;
        public Dialogue CurrentDialogue { get; private set; }
        public DialogueNode CurrentNode { get; private set; }
        
        public event Action<Dialogue> DialogueStarted;
        public event Action<DialogueNode> NodeChanged;
        public event Action<Dialogue> DialogueEnded;

        public DialogueRunner(IDialogueDatabase dialogueDatabase)
        {
            this.dialogueDatabase = dialogueDatabase
                ?? throw new ArgumentNullException(nameof(dialogueDatabase));
        }
        
        public void Start(string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                throw new ArgumentException(
                    "Dialogue ID cannot be null or empty.",
                    nameof(dialogueId));
            }

            Dialogue dialogue = dialogueDatabase.GetDialogue(dialogueId);

            if (dialogue == null)
            {
                throw new InvalidOperationException(
                    $"Dialogue database returned null for '{dialogueId}'.");
            }

            if (!dialogue.TryGetNode(dialogue.StartNodeId, out DialogueNode startNode))
            {
                throw new InvalidOperationException(
                    $"Dialogue '{dialogue.Id}' has no start node " +
                    $"'{dialogue.StartNodeId}'.");
            }

            CurrentDialogue = dialogue;
            CurrentNode = null;
            State = DialogueRuntimeState.Idle;
            
            DialogueStarted?.Invoke(CurrentDialogue);

            EnterNode(startNode);
        }

        public void Continue()
        {
            if (State != DialogueRuntimeState.AwaitingInput)
            {
                throw new InvalidOperationException(
                    $"Cannot continue dialogue while state is '{State}'. " +
                    "The runner must be awaiting input.");
            }

            if (CurrentNode == null)
            {
                throw new InvalidOperationException(
                    "Cannot continue dialogue because the current node is null.");
            }

            if (CurrentNode.Type != DialogueNodeType.Line)
            {
                throw new InvalidOperationException(
                    $"Cannot continue from node '{CurrentNode.Id}' " +
                    $"of type '{CurrentNode.Type}'.");
            }
            
            if (string.IsNullOrWhiteSpace(CurrentNode.NextNodeId))
            {
                EndDialogue();
                return;
            }
            
            EnterNode(ResolveNode(CurrentNode.NextNodeId));

            
        }

        public void SelectChoice(string choiceId)
        {
            if (State != DialogueRuntimeState.AwaitingChoice)
            {
                throw new InvalidOperationException(
                    $"Cannot select a choice while state is '{State}'. " +
                    "The runner must be awaiting a choice.");
            }

            if (string.IsNullOrWhiteSpace(choiceId))
            {
                throw new ArgumentException(
                    "Choice ID cannot be null or empty.",
                    nameof(choiceId));
            }

            if (CurrentNode == null ||
                CurrentNode.Type != DialogueNodeType.Choice)
            {
                throw new InvalidOperationException(
                    "Cannot select a choice because the current node " +
                    "is not a Choice node.");
            }

            DialogueChoice selectedChoice = CurrentNode.Choices
                .FirstOrDefault(choice => choice.Id == choiceId);

            if (selectedChoice == null)
            {
                throw new InvalidOperationException(
                    $"Choice '{choiceId}' does not exist in node " +
                    $"'{CurrentNode.Id}' of dialogue '{CurrentDialogue.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(selectedChoice.NextNodeId))
            {
                EndDialogue();
                return;
            }

            EnterNode(ResolveNode(selectedChoice.NextNodeId));
        }

        public void Stop()
        {
            if (CurrentDialogue == null)
            {
                State = DialogueRuntimeState.Idle;
                CurrentNode = null;
                return;
            }

            EndDialogue();
        }

        private void EnterNode(DialogueNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            CurrentNode = node;

            switch (node.Type)
            {
                case DialogueNodeType.Line:
                    State = DialogueRuntimeState.AwaitingInput;
                    break;

                case DialogueNodeType.Choice:
                    if (node.Choices == null || node.Choices.Count == 0)
                    {
                        throw new InvalidOperationException(
                            $"Choice node '{node.Id}' in dialogue " +
                            $"'{CurrentDialogue.Id}' has no choices.");
                    }

                    State = DialogueRuntimeState.AwaitingChoice;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(node.Type),
                        node.Type,
                        $"Unsupported dialogue node type: {node.Type}.");
            }

            NodeChanged?.Invoke(CurrentNode);
        }

        private DialogueNode ResolveNode(string nodeId)
        {
            if (CurrentDialogue == null)
            {
                throw new InvalidOperationException(
                    "Cannot resolve a node because no dialogue is active.");
            }

            if (!CurrentDialogue.TryGetNode(nodeId, out DialogueNode node))
            {
                throw new InvalidOperationException(
                    $"Dialogue '{CurrentDialogue.Id}' references missing node " +
                    $"'{nodeId}'.");
            }

            return node;
        }

        private void EndDialogue()
        {
            Dialogue dialogue = CurrentDialogue;

            CurrentDialogue = null;
            CurrentNode = null;
            State = DialogueRuntimeState.Ended;

            DialogueEnded?.Invoke(dialogue);
        }
    }
}