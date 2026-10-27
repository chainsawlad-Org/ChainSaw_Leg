using ChainSawLeg.Application;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class StartDialogueOnInteract : MonoBehaviour
    {
        [SerializeField] private string dialogueId;

        private GameStateMachine gameStateMachine;

        [Inject]
        public void Construct(GameStateMachine stateMachine)
        {
            gameStateMachine = stateMachine;
        }

        public void StartDialogue()
        {
            if (gameStateMachine == null)
            {
                Debug.LogError(
                    "[StartDialogueOnInteract] GameStateMachine was not injected.",
                    this);

                return;
            }
            
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                Debug.LogError(
                    "[StartDialogueOnInteract] Dialogue ID is empty.",
                    this);

                return;
            }
            
            if (gameStateMachine.HasOverlayOfType<DialoguePhase>())
            {
                return;
            }

            gameStateMachine
                .PushOverlay<DialoguePhase>(phase => phase.Configure(dialogueId))
                .Forget();
        }
    }
}