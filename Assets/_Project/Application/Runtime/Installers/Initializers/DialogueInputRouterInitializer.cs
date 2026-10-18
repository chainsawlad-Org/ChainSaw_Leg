using ChainSawLeg.Features.Dialogue;
using ChainSawLeg.Game.Shared;
using Zenject;

namespace ChainSawLeg.Composition
{
    public sealed class DialogueInputRouterInitializer : IInitializable
    {
        private readonly DialogueInputRouter dialogueInputRouter;
        private readonly IGameInputService inputService;
        private readonly IDialogueRunner dialogueRunner;
        private readonly DialogueUI_RPG dialogueUI;

        public DialogueInputRouterInitializer(
            DialogueInputRouter dialogueInputRouter,
            IGameInputService inputService,
            IDialogueRunner dialogueRunner,
            DialogueUI_RPG dialogueUI)
        {
            this.dialogueInputRouter = dialogueInputRouter;
            this.inputService = inputService;
            this.dialogueRunner = dialogueRunner;
            this.dialogueUI = dialogueUI;
        }

        public void Initialize()
        {
            dialogueInputRouter.Initialize(
                inputService,
                dialogueRunner,
                dialogueUI);
        }
    }
}