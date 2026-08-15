using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChainSawLeg.Composition
{
    public class SimpleDialogue : NpcDialogue
    {
        [TextArea(2, 5)] [SerializeField] private string[] nodes;

        private List<IDialogueEvent> Dialogue()
        {
            DialogueNode prevNode = null;
            DialogueNode startNode = null;
            for (int i = 0; i < nodes.Length; i++)
            {
                DialogueNode node = new DialogueNode()
                {
                    text = nodes[i]
                };
                if (prevNode != null) prevNode.nextNode = node;
                prevNode = node;
                
                if (i == 0) startNode = node;
            }
            
            return DialogueLibrary.ConvertToEvents(startNode);
        }
        
        public override void Interact()
        {
            if (!CanInteract())
                return;

            StartDialogue(destroyCancellationToken, Dialogue()).Forget();
        }
    }
}
        
