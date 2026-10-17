using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueValidator
    {
        public bool Validate(DialogueJsonData data)
        {
            if (data.Dialogues == null)
            {
                Debug.LogError(
                    "Dialogue JSON does not contain 'Dialogues'.");

                return false;
            }

            if (data.Nodes == null)
            {
                Debug.LogError(
                    "Dialogue JSON does not contain 'Nodes'");

                return false;
            }

            if (data.Choices == null)
            {
                Debug.LogError(
                    "Dialogue JSON does not contain 'Choices'");

                return false;
            }

            if (!ValidateDialogues(data))
            {
                return false;
            }

            if (!ValidateNodes(data))
            {
                return false;
            }

            if (!ValidateChoices(data))
            {
                return false;
            }

            return true;
        }

        private bool ValidateDialogues(DialogueJsonData data)
        {
            HashSet<string> dialoguesIds = new HashSet<string>();

            foreach (DialogueJsonEntry dialogue in data.Dialogues)
            {
                if (dialogue == null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(dialogue.dialogue_id))
                {
                    Debug.LogError(
                        "Dialogue has empty 'dialogue_id'.");

                    return false;
                }

                if (!dialoguesIds.Add(dialogue.dialogue_id))
                {
                    Debug.LogError(
                        $"Duplicate dialogue ID: " +
                        $"'{dialogue.dialogue_id}'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        dialogue.title))
                {
                    Debug.LogError(
                        $"Dialogue '{dialogue.dialogue_id}' " +
                        $"has empty 'title'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        dialogue.start_node_id))
                {
                    Debug.LogError(
                        $"Dialogue '{dialogue.dialogue_id}' " +
                        $"has empty 'start_node_id'.");

                    return false;
                }
            }

            return true;
        }

        private bool ValidateNodes(DialogueJsonData data)
        {
            HashSet<string> dialogueIds = new();

            foreach (DialogueJsonEntry dialogue in data.Dialogues)
            {
                dialogueIds.Add(dialogue.dialogue_id);
            }

            /*
             * node_id is NOT globally unique.
             *
             * The actual identity of a node is:
             *
             *     dialogue_id + node_id
             *
             * Example:
             *
             *     guard_intro:node_001
             *     toilet_wars_intro:node_001
             *
             * These are two different nodes.
             */
            HashSet<string> nodeKeys = new();

            foreach (DialogueNodeJsonEntry node in data.Nodes)
            {
                if (node == null)
                {
                    Debug.LogError(
                        "Dialogue JSON contains a null node entry.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        node.node_id))
                {
                    Debug.LogError(
                        "Node has empty 'node_id'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        node.dialogue_id))
                {
                    Debug.LogError(
                        $"Node '{node.node_id}' " +
                        $"has empty 'dialogue_id'.");

                    return false;
                }

                if (!dialogueIds.Contains(
                        node.dialogue_id))
                {
                    Debug.LogError(
                        $"Node '{node.node_id}' references " +
                        $"non-existent dialogue " +
                        $"'{node.dialogue_id}'.");

                    return false;
                }

                string nodeKey = CreateNodeKey(
                    node.dialogue_id,
                    node.node_id);

                if (!nodeKeys.Add(nodeKey))
                {
                    Debug.LogError(
                        $"Duplicate node ID '{node.node_id}' " +
                        $"inside dialogue " +
                        $"'{node.dialogue_id}'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        node.type))
                {
                    Debug.LogError(
                        $"Node '{node.node_id}' " +
                        $"has empty 'type'.");

                    return false;
                }

                if (!Enum.TryParse(
                        node.type,
                        true,
                        out DialogueNodeType _))
                {
                    Debug.LogError(
                        $"Node '{node.node_id}' " +
                        $"in dialogue '{node.dialogue_id}' " +
                        $"has invalid type '{node.type}'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        node.speaker))
                {
                    Debug.LogError(
                        $"Node '{node.node_id}' " +
                        $"has empty 'speaker'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        node.text))
                {
                    Debug.LogError(
                        $"Node '{node.node_id}' " +
                        $"has empty 'text'.");

                    return false;
                }
            }

            /*
             * Validate next_node_id.
             *
             * A node can only point to another node
             * inside the SAME dialogue.
             */
            foreach (DialogueNodeJsonEntry node in data.Nodes)
            {
                if (string.IsNullOrWhiteSpace(
                        node.next_node_id))
                {
                    continue;
                }

                string targetKey = CreateNodeKey(
                    node.dialogue_id,
                    node.next_node_id);

                if (!nodeKeys.Contains(targetKey))
                {
                    Debug.LogError(
                        $"Node '{node.node_id}' " +
                        $"in dialogue '{node.dialogue_id}' " +
                        $"references non-existent next node " +
                        $"'{node.next_node_id}'.");

                    return false;
                }
            }

            /*
             * Validate start_node_id.
             *
             * The start node must belong to the same dialogue.
             */
            foreach (DialogueJsonEntry dialogue in data.Dialogues)
            {
                string startNodeKey = CreateNodeKey(
                    dialogue.dialogue_id,
                    dialogue.start_node_id);

                if (!nodeKeys.Contains(startNodeKey))
                {
                    Debug.LogError(
                        $"Dialogue '{dialogue.dialogue_id}' " +
                        $"references non-existent start node " +
                        $"'{dialogue.start_node_id}'.");

                    return false;
                }
            }

            return true;
        }

        private bool ValidateChoices(DialogueJsonData data)
        {
            HashSet<string> dialogueIds = new();

            foreach (DialogueJsonEntry dialogue in data.Dialogues)
            {
                dialogueIds.Add(dialogue.dialogue_id);
            }

            HashSet<string> nodeKeys = new();

            Dictionary<string, DialogueNodeType> nodeTypes = new();

            foreach (DialogueNodeJsonEntry node in data.Nodes)
            {
                string nodeKey = CreateNodeKey(
                    node.dialogue_id,
                    node.node_id);

                nodeKeys.Add(nodeKey);

                nodeTypes[nodeKey] =
                    Enum.Parse<DialogueNodeType>(
                        node.type,
                        true);
            }

            HashSet<string> choiceIds = new();

            foreach (DialogueChoiceJsonEntry choice in data.Choices)
            {
                if (choice == null)
                {
                    Debug.LogError(
                        "Dialogue JSON contains a null choice entry.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        choice.choice_id))
                {
                    Debug.LogError(
                        "Choice has empty 'choice_id'.");

                    return false;
                }

                if (!choiceIds.Add(
                        choice.choice_id))
                {
                    Debug.LogError(
                        $"Duplicate choice ID: " +
                        $"'{choice.choice_id}'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        choice.dialogue_id))
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"has empty 'dialogue_id'.");

                    return false;
                }

                if (!dialogueIds.Contains(
                        choice.dialogue_id))
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"references non-existent dialogue " +
                        $"'{choice.dialogue_id}'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        choice.node_id))
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"has empty 'node_id'.");

                    return false;
                }

                string ownerNodeKey = CreateNodeKey(
                    choice.dialogue_id,
                    choice.node_id);

                if (!nodeKeys.Contains(ownerNodeKey))
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"references non-existent node " +
                        $"'{choice.node_id}' " +
                        $"in dialogue " +
                        $"'{choice.dialogue_id}'.");

                    return false;
                }

                /*
                 * Only Choice nodes may have choices.
                 */
                if (nodeTypes[ownerNodeKey]
                    != DialogueNodeType.Choice)
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"belongs to node '{choice.node_id}' " +
                        $"in dialogue '{choice.dialogue_id}', " +
                        $"but this node is not a Choice node.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        choice.text))
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"has empty 'text'.");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(
                        choice.next_node_id))
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"has empty 'next_node_id'.");

                    return false;
                }

                /*
                 * Choice target must also belong to
                 * the SAME dialogue.
                 */
                string targetNodeKey = CreateNodeKey(
                    choice.dialogue_id,
                    choice.next_node_id);

                if (!nodeKeys.Contains(targetNodeKey))
                {
                    Debug.LogError(
                        $"Choice '{choice.choice_id}' " +
                        $"in dialogue '{choice.dialogue_id}' " +
                        $"references non-existent next node " +
                        $"'{choice.next_node_id}'.");

                    return false;
                }
            }

            return true;
        }
        
        private static string CreateNodeKey(
            string dialogueId,
            string nodeId)
        {
            return $"{dialogueId}:{nodeId}";
        }
    }
}
