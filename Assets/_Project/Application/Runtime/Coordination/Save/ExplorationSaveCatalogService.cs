using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationSaveCatalogService
    {
        private readonly IGameSaveStorageProvider storageProvider;
        private readonly GameSaveCoordinator saveCoordinator;

        public ExplorationSaveCatalogService(
            IGameSaveStorageProvider storageProvider,
            GameSaveCoordinator saveCoordinator)
        {
            this.storageProvider = storageProvider;
            this.saveCoordinator = saveCoordinator;
        }

        public async UniTask<IReadOnlyList<GameSaveCatalogEntry>> GetCheckpointEntriesAsync(
            CancellationToken cancellationToken)
        {
            IReadOnlyList<GameSaveSlotInfo> slots = await storageProvider.ListSlotsAsync(cancellationToken);
            Dictionary<string, GameSaveSlotInfo> slotsById = slots
                .Where(IsCheckpointSlot)
                .ToDictionary(slot => slot.SlotId, StringComparer.Ordinal);

            var entries = new List<GameSaveCatalogEntry>(GameSaveSlotCatalog.CheckpointSlotIds.Count);

            foreach (string slotId in GameSaveSlotCatalog.CheckpointSlotIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!slotsById.TryGetValue(slotId, out GameSaveSlotInfo slot))
                    continue;

                entries.Add(await CreateEntryAsync(slot, cancellationToken));
            }

            return entries;
        }

        public async UniTask<IReadOnlyList<GameSaveCatalogEntry>> GetEntriesAsync(
            CancellationToken cancellationToken)
        {
            IReadOnlyList<GameSaveSlotInfo> slots = await storageProvider.ListSlotsAsync(cancellationToken);
            var entries = new List<GameSaveCatalogEntry>(slots.Count);

            foreach (GameSaveSlotInfo slot in slots)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (slot != null)
                    entries.Add(await CreateEntryAsync(slot, cancellationToken));
            }

            return entries;
        }

        public async UniTask<IReadOnlyList<GameSaveCatalogEntry>> GetCheckpointSaveMenuEntriesAsync(
            CancellationToken cancellationToken)
        {
            IReadOnlyList<GameSaveSlotInfo> slots = await storageProvider.ListSlotsAsync(cancellationToken);
            Dictionary<string, GameSaveSlotInfo> slotsById = slots
                .Where(IsCheckpointSlot)
                .ToDictionary(slot => slot.SlotId, StringComparer.Ordinal);

            var entries = new List<GameSaveCatalogEntry>(GameSaveSlotCatalog.CheckpointSlotIds.Count);

            foreach (string slotId in GameSaveSlotCatalog.CheckpointSlotIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!slotsById.TryGetValue(slotId, out GameSaveSlotInfo slot))
                {
                    entries.Add(CreateEmptyEntry(slotId));
                    continue;
                }

                entries.Add(await CreateEntryAsync(slot, cancellationToken));
            }

            return entries;
        }

        private async UniTask<GameSaveCatalogEntry> CreateEntryAsync(
            GameSaveSlotInfo slot,
            CancellationToken cancellationToken)
        {
            if (slot.IsCorrupted || slot.Metadata == null)
                return CreateCorruptedEntry(slot.SlotId);

            try
            {
                var request = new GameSaveRequest(slot.Metadata.Kind, slot.SlotId);
                GameSaveData saveData = await saveCoordinator.ReadAsync(request, cancellationToken);
                ExplorationSaveData explorationData = saveCoordinator.ReadContributorData<ExplorationSaveData>(
                    saveData,
                    ExplorationSaveContributor.Id);

                return new GameSaveCatalogEntry(
                    slot.Metadata.Kind,
                    slot.SlotId,
                    explorationData.CheckpointId,
                    explorationData.SceneId,
                    GetSceneName(explorationData.SceneId),
                    slot.Metadata.UtcTimestamp,
                    isLoadable: true);
            }
            catch (GameSaveException)
            {
                return CreateCorruptedEntry(slot.SlotId);
            }
        }

        private static bool IsCheckpointSlot(GameSaveSlotInfo slot)
        {
            return slot != null &&
                   GameSaveSlotCatalog.CheckpointSlotIds.Contains(slot.SlotId) &&
                   (slot.Metadata == null || slot.Metadata.Kind == GameSaveKind.Checkpoint);
        }

        private static GameSaveCatalogEntry CreateEmptyEntry(string slotId)
        {
            return new GameSaveCatalogEntry(
                GameSaveKind.Checkpoint,
                slotId,
                checkpointId: null,
                sceneId: string.Empty,
                sceneName: "—",
                utcTimestamp: DateTime.MinValue,
                isLoadable: false,
                isEmpty: true);
        }

        private static GameSaveCatalogEntry CreateCorruptedEntry(string slotId)
        {
            return new GameSaveCatalogEntry(
                GameSaveKind.Checkpoint,
                slotId,
                checkpointId: "Повреждено",
                sceneId: string.Empty,
                sceneName: "—",
                utcTimestamp: DateTime.MinValue,
                isLoadable: false);
        }

        private static string GetSceneName(string sceneId)
        {
            try
            {
                return ExplorationSceneResolver.ResolveSceneName(sceneId);
            }
            catch (GameSaveValidationException)
            {
                return sceneId;
            }
        }
    }
}
