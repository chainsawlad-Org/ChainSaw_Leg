using ChainSawLeg.Game.Shared;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueInputRouter : MonoBehaviour
    {
        private IGameInputService inputService;
        private IDialogueRunner dialogueRunner;
        private DialogueUI_RPG dialogueUI;

        private bool isInitialized;
        private bool isSubscribed;

        public void Initialize(
            IGameInputService inputService,
            IDialogueRunner dialogueRunner,
            DialogueUI_RPG dialogueUI)
        {
            if (isInitialized)
                return;

            this.inputService = inputService;
            this.dialogueRunner = dialogueRunner;
            this.dialogueUI = dialogueUI;

            isInitialized = true;

            Subscribe();
        }
        
        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            if (dialogueRunner != null)
            {
                Unsubscribe();
            }
        }
        
        private void Subscribe()
        {
            if (!isInitialized || !isActiveAndEnabled || isSubscribed)
                return;

            dialogueRunner.NodeChanged += OnNodeChanged;
            dialogueRunner.DialogueEnded += OnDialogueEnded;
            dialogueRunner.DialogueStarted += OnDialogueStarted;

            isSubscribed = true;
        }
        
        private void Unsubscribe()
        {
            if (!isSubscribed)
                return;

            dialogueRunner.NodeChanged -= OnNodeChanged;
            dialogueRunner.DialogueEnded -= OnDialogueEnded;
            dialogueRunner.DialogueStarted -= OnDialogueStarted;

            isSubscribed = false;
        }

        private void Update()
        {
            if (!isInitialized)
                return;

            switch (dialogueRunner.State)
            {
                case DialogueRuntimeState.AwaitingInput:
                    HandleConfirmation();
                    break;

                case DialogueRuntimeState.AwaitingChoice:
                    HandleChoiceNavigation();
                    HandleConfirmation();
                    break;
            }
        }

        private void HandleChoiceNavigation()
        {
            if (inputService.PreviousPressed)
            {
                inputService.ConsumePrevious();
                dialogueUI.SelectPreviousChoice();
            }

            if (inputService.NextPressed)
            {
                inputService.ConsumeNext();
                dialogueUI.SelectNextChoice();
            }
        }

        private void HandleConfirmation()
        {
            if (!inputService.UiSubmitPressed)
                return;

            inputService.ConsumeSubmit();

            switch (dialogueRunner.State)
            {
                case DialogueRuntimeState.AwaitingInput:
                    dialogueRunner.Continue();
                    break;

                case DialogueRuntimeState.AwaitingChoice:
                    if (dialogueUI.TryGetSelectedChoiceId(
                            out string choiceId))
                    {
                        dialogueRunner.SelectChoice(choiceId);
                    }

                    break;
            }
        }

        private void OnNodeChanged(DialogueNode node)
        {
            dialogueUI.ShowNode(node);
        }

        private void OnDialogueEnded(Dialogue dialogue)
        {
            dialogueUI.Hide();
        }
        
        private void OnDialogueStarted(Dialogue dialogue)
        {
            dialogueUI.BeginDialogue();
        }
    }
}
