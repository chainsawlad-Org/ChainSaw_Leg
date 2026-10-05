using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChainSawLeg.Composition
{
    public class WardrobeDialogue : NpcDialogue
    {
        private int appleCount = 2;
        
        private List<IDialogueEvent> Dialogue()
        {
            var getFirstApple = new DialogueNode
            {
                text = "Вы забрали 1 яблоко."
            };
            
            var getSecondAppleEnd = new DialogueNode()
            {
                text = "Вы чувствуете себя настоящим злодеем, крадя чужие яблоки."
            };

            var getSecondApple = new DialogueNode()
            {
                text = "Вы забрали 1 яблоко.",
                nextNode = getSecondAppleEnd
            };
            
            var getThirdApple = new DialogueNode()
            {
                text = "Пожалуй вам своих уже достаточно."
            };

            var getApple = getFirstApple;
            if (appleCount == 1) getApple = getSecondApple;
            if (appleCount <= 0)  getApple = getThirdApple;
            
            var question = new DialogueNode
            {
                text = "Взять яблоко?",
                choices = new List<DialogueChoice>
                {
                    new DialogueChoice { text = "Да", nextNode = getApple, command = () => appleCount-- },
                    new DialogueChoice { text = "Нет" }
                }
            };
            
            var startNode = new DialogueNode
            {
                text = "Внутри шкафчика вы обнаружили несколько яблок.",
                nextNode = question
            };
            
            
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
