using System.Threading;
using ChainSawLeg.Game.Shared;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Application
{
    public sealed class IntroPhase : SceneGamePhase
    {
        private readonly IntroSequenceService introSequenceService;

        protected override string SceneName => SceneNames.Intro;

        public override bool AllowsGameplayInput => false;

        public IntroPhase(
            ISceneLoader sceneLoader,
            IntroSequenceService introSequenceService)
            : base(sceneLoader)
        {
            this.introSequenceService = introSequenceService;
        }

        public override async UniTask EnterAsync(
            CancellationToken cancellationToken)
        {
            introSequenceService.Prepare();

            await base.EnterAsync(cancellationToken);

            await introSequenceService.WaitForCompletionAsync(
                cancellationToken);
        }
    }
}