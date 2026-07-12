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

                return new GameSaveCatalogEntry
                {
                    Kind = slot.Metadata.Kind,
                    SlotId = slot.SlotId,
                    CheckpointId = explorationData.CheckpointId,
                    SceneId = explorationData.SceneId,
                    SceneName = GetSceneName(explorationData.SceneId),
                    UtcTimestamp = slot.Metadata.UtcTimestamp,
                    IsLoadable = true
                };
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
            return new GameSaveCatalogEntry
            {
                Kind = GameSaveKind.Checkpoint,
                SlotId = slotId,
                CheckpointId = null,
                SceneId = string.Empty,
                SceneName = "—",
                UtcTimestamp = DateTime.MinValue,
                IsLoadable = false,
                IsEmpty = true
            };
        }

        private static GameSaveCatalogEntry CreateCorruptedEntry(string slotId)
        {
            return new GameSaveCatalogEntry
            {
                Kind = GameSaveKind.Checkpoint,
                SlotId = slotId,
                CheckpointId = "Повреждено",
                SceneId = string.Empty,
                SceneName = "—",
                UtcTimestamp = DateTime.MinValue,
                IsLoadable = false
            };
        }

        private static string GetSceneName(string sceneId)
        {
            try
            {
                return ExplorationSceneIds.ResolveSceneName(sceneId);
            }
            catch (GameSaveValidationException)
            {
                return sceneId;
            }
        }
    }
}
