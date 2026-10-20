using System;
using ChainSawLeg.Features.Dialogue;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Application
{
    public class DialoguePhase : OverlayPhase
    {
        private IDialogueRunner dialogueRunner;
        private GameStateMachine gameStateMachine;

        private string dialogueId;
        private bool isActive;
        
        private readonly DialogueViewRegistry dialogueViewRegistry;

        public override bool CanStack => false;

        public override InputBlockChannels BlockedInputChannels =>
            InputBlockChannels.Move |
            InputBlockChannels.Dash |
            InputBlockChannels.Interact;

        public DialoguePhase(
            IDialogueRunner dialogueRunner,
            GameStateMachine gameStateMachine,
            DialogueViewRegistry dialogueViewRegistry)
        {
            this.dialogueRunner = dialogueRunner
                                  ?? throw new ArgumentNullException(nameof(dialogueRunner));

            this.gameStateMachine = gameStateMachine
                                    ?? throw new ArgumentNullException(nameof(gameStateMachine));
            
            this.dialogueViewRegistry = dialogueViewRegistry
                                        ?? throw new ArgumentNullException(nameof(dialogueViewRegistry));
        }

        public void Configure(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Dialogue ID cannot be empty.",
                    nameof(id));
            }

            dialogueId = id;
        }

        public override UniTask Enter()
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                throw new InvalidOperationException(
                    "DialoguePhase was entered without a dialogue ID.");
            }
            
            if (dialogueRunner.CurrentDialogue != null)
            {
                throw new InvalidOperationException(
                    "Cannot start a dialogue because another dialogue is active.");
            }

            dialogueRunner.DialogueEnded += OnDialogueEnded;
            isActive = true;

            try
            {
                dialogueRunner.Start(dialogueId);
            }
            catch
            {
                isActive = false;
                dialogueRunner.DialogueEnded -= OnDialogueEnded;
                throw;
            }

            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            isActive = false;

            dialogueRunner.DialogueEnded -= OnDialogueEnded;

            if (dialogueRunner.CurrentDialogue == null)
            {
                dialogueRunner.Stop();
            }

            dialogueId = null;

            return UniTask.CompletedTask;
        }

        private void OnDialogueEnded(Dialogue dialogue)
        {
            if (!isActive)
                return;

            ClosePhaseAsync().Forget();
        }

        private async UniTask ClosePhaseAsync()
        {
            if (!isActive)
                return;
            
            if (!gameStateMachine.IsTopOverlay<DialoguePhase>())
                return;
            
            await dialogueViewRegistry.Get().HideAnimatedAsync();
            
            if (!isActive)
                return;

            await gameStateMachine.PopOverlay();
        }
    }
}
