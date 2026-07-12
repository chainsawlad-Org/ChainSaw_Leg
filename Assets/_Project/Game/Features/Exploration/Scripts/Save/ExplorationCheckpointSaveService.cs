using System;
using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationCheckpointSaveService : IDisposable
    {
        private readonly GameSaveCoordinator saveCoordinator;
        private readonly CheckpointGameSaveSlotRotationService slotRotationService;
        private readonly ExplorationSaveContextService saveContextService;
        private readonly IGameSaveRuntimeMetadataProvider metadataProvider;
        private readonly SemaphoreSlim saveLock = new(1, 1);

        public bool IsCoordinatorRegistered => saveCoordinator != null;

        public ExplorationCheckpointSaveService(
            GameSaveCoordinator saveCoordinator,
            CheckpointGameSaveSlotRotationService slotRotationService,
            ExplorationSaveContextService saveContextService,
            IGameSaveRuntimeMetadataProvider metadataProvider)
        {
            this.saveCoordinator = saveCoordinator;
            this.slotRotationService = slotRotationService;
            this.saveContextService = saveContextService;
            this.metadataProvider = metadataProvider;
        }

        public async UniTask SaveCheckpointAsync(
            string checkpointId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(checkpointId))
                throw new GameSaveValidationException("Checkpoint ID is required.");

            await saveLock.WaitAsync(cancellationToken);

            try
            {
                string slotId = await slotRotationService.GetNextSlotIdAsync(cancellationToken);
                await SaveToSlotCoreAsync(slotId, checkpointId, cancellationToken);
            }
            finally
            {
                saveLock.Release();
            }
        }

        public async UniTask SaveCheckpointToSlotAsync(
            string slotId,
            string checkpointId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(slotId))
                throw new GameSaveValidationException("Slot ID is required.");

            if (string.IsNullOrWhiteSpace(checkpointId))
                throw new GameSaveValidationException("Checkpoint ID is required.");

            await saveLock.WaitAsync(cancellationToken);

            try
            {
                await SaveToSlotCoreAsync(slotId, checkpointId, cancellationToken);
            }
            finally
            {
                saveLock.Release();
            }
        }

        private async UniTask SaveToSlotCoreAsync(
            string slotId,
            string checkpointId,
            CancellationToken cancellationToken)
        {
            saveContextService.SetContext(saveContextService.SceneId, checkpointId);

            var request = new GameSaveRequest(
                GameSaveKind.Checkpoint,
                slotId,
                checkpointId);

            await saveCoordinator.SaveAsync(
                request,
                metadataProvider.ProfileId,
                metadataProvider.BuildNumber,
                cancellationToken);
        }

        public void Dispose()
        {
            saveLock.Dispose();
        }
    }
}
