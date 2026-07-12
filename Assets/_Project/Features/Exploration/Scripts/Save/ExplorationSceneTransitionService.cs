using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationSceneTransitionService : IExplorationSceneTransitionService
    {
        private readonly GameStateMachine gameStateMachine;

        public ExplorationSceneTransitionService(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }

        public UniTask ReloadSceneAsync(string sceneId, CancellationToken cancellationToken)
        {
            string sceneName = ExplorationSceneIds.ResolveSceneName(sceneId);

            return gameStateMachine.ReloadMainAsync<ExplorationPhase>(
                phase => phase.SetTargetScene(sceneName),
                cancellationToken);
        }
    }
}
