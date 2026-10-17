using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueInputRouter : MonoBehaviour
    {
        // private InputService inputService;
        // private DialogueRuntimeRegistry runtimeRegistry;

        private IGameInputService inputService;
        private IDialogueRunner dialogueRunner;
        private DialogueUI_RPG dialogueUI;

        [Inject]
        // public void Construct(
        //     InputService inputService,
        //     DialogueRuntimeRegistry runtimeRegistry)
        // {
        //     this.inputService = inputService;
        //     this.runtimeRegistry = runtimeRegistry;
        // }

        public void Construct(IGameInputService inputService, IDialogueRunner dialogueRunner, DialogueUI_RPG dialogueUI)
        {
            this.inputService = inputService;
            this.dialogueRunner = dialogueRunner;
            this.dialogueUI = dialogueUI;
            
            this.dialogueRunner.NodeChanged += OnNodeChanged;
            this.dialogueRunner.DialogueEnded += OnDialogueEnded;
        }

        private void OnDisable()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.NodeChanged -= OnNodeChanged;
                dialogueRunner.DialogueEnded -= OnDialogueEnded;
            }
        }

        private void Update()
        {
            if (inputService == null || dialogueRunner == null || dialogueUI == null)
            {
                return;
            }

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
    }
}
