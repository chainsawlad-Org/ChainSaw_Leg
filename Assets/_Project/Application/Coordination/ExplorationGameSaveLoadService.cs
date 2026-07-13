using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationGameSaveLoadService
    {
        private readonly GameSaveCoordinator saveCoordinator;
        private readonly GameSavePendingRestoreService pendingRestoreService;
        private readonly ExplorationPlayerRegistry playerRegistry;
        private readonly ExplorationSaveContextService saveContextService;
        private readonly IExplorationSceneTransitionService sceneTransitionService;
        private readonly BattleSessionService battleSessionService;

        public ExplorationGameSaveLoadService(
            GameSaveCoordinator saveCoordinator,
            GameSavePendingRestoreService pendingRestoreService,
            ExplorationPlayerRegistry playerRegistry,
            ExplorationSaveContextService saveContextService,
            IExplorationSceneTransitionService sceneTransitionService,
            BattleSessionService battleSessionService)
        {
            this.saveCoordinator = saveCoordinator;
            this.pendingRestoreService = pendingRestoreService;
            this.playerRegistry = playerRegistry;
            this.saveContextService = saveContextService;
            this.sceneTransitionService = sceneTransitionService;
            this.battleSessionService = battleSessionService;
        }

        public async UniTask LoadAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            GameSaveData saveData = await saveCoordinator.ReadAsync(request, cancellationToken);
            ExplorationSaveData explorationData = saveCoordinator.ReadContributorData<ExplorationSaveData>(
                saveData,
                ExplorationSaveContributor.Id);
            Validate(explorationData);

            int previousRegistrationVersion = playerRegistry.RegistrationVersion;
            pendingRestoreService.SetPending(saveData);
            battleSessionService.Reset();

            try
            {
                saveContextService.SetContext(
                    explorationData.SceneId,
                    explorationData.CheckpointId);
                await sceneTransitionService.ReloadSceneAsync(
                    explorationData.SceneId,
                    cancellationToken);
                await playerRegistry.WaitForRegistrationAfterAsync(
                    previousRegistrationVersion,
                    cancellationToken);
                saveCoordinator.Restore(
                    pendingRestoreService.GetPending(),
                    cancellationToken);
                pendingRestoreService.Clear();
            }
            catch
            {
                pendingRestoreService.Clear();
                throw;
            }
        }

        private static void Validate(ExplorationSaveData saveData)
        {
            if (saveData == null || string.IsNullOrWhiteSpace(saveData.SceneId))
                throw new GameSaveValidationException("Exploration save scene ID is required.");

            ExplorationSceneResolver.ResolveSceneName(saveData.SceneId);

            if (float.IsNaN(saveData.PositionX) || float.IsInfinity(saveData.PositionX) ||
                float.IsNaN(saveData.PositionY) || float.IsInfinity(saveData.PositionY))
                throw new GameSaveValidationException("Exploration player position is invalid.");
        }
    }
}
