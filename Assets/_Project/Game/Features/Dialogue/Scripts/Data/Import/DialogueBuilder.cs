
using System;
using System.Collections.Generic;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueBuilder
    {
         public List<Dialogue> Build(DialogueJsonData data)
        {
            Dictionary<string, List<DialogueChoice>>
                choicesByNode = BuildChoices(data);

            List<Dialogue> dialogues =
                new(data.Dialogues.Count);

            foreach (DialogueJsonEntry dialogueData in data.Dialogues)
            {
                Dictionary<string, DialogueNode>
                    nodes = new();

                foreach (DialogueNodeJsonEntry nodeData
                         in data.Nodes)
                {
                    if (nodeData.dialogue_id
                        != dialogueData.dialogue_id)
                    {
                        continue;
                    }

                    string nodeKey = CreateNodeKey(
                        nodeData.dialogue_id,
                        nodeData.node_id);

                    List<DialogueChoice> choices =
                        new();

                    if (choicesByNode.TryGetValue(
                            nodeKey,
                            out List<DialogueChoice>
                                nodeChoices))
                    {
                        choices = nodeChoices;
                    }

                    DialogueNodeType nodeType =
                        Enum.Parse<DialogueNodeType>(
                            nodeData.type,
                            true);

                    DialogueNode node =
                        new DialogueNode(
                            nodeData.node_id,
                            nodeType,
                            nodeData.speaker,
                            nodeData.text,
                            nodeData.next_node_id,
                            choices);

                    nodes.Add(
                        node.Id,
                        node);
                }

                Dialogue dialogue =
                    new Dialogue(
                        dialogueData.dialogue_id,
                        dialogueData.title,
                        dialogueData.start_node_id,
                        nodes);

                dialogues.Add(dialogue);
            }

            return dialogues;
        }
        
        private Dictionary<string, List<DialogueChoice>> BuildChoices(DialogueJsonData data)
        {
            Dictionary<string, List<DialogueChoice>>
                result = new();

            foreach (DialogueChoiceJsonEntry choiceData
                     in data.Choices)
            {
                string nodeKey = CreateNodeKey(
                    choiceData.dialogue_id,
                    choiceData.node_id);

                if (!result.TryGetValue(
                        nodeKey,
                        out List<DialogueChoice> choices))
                {
                    choices = new List<DialogueChoice>();

                    result.Add(
                        nodeKey,
                        choices);
                }

                DialogueChoice choice =
                    new DialogueChoice(
                        choiceData.choice_id,
                        choiceData.text,
                        choiceData.next_node_id);

                choices.Add(choice);
            }

            return result;
        }
        
        private static string CreateNodeKey(
            string dialogueId,
            string nodeId)
        {
            return $"{dialogueId}:{nodeId}";
        }
    }
}
