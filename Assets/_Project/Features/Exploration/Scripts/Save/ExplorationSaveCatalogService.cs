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
            var entries = new List<GameSaveCatalogEntry>(GameSaveSlotCatalog.CheckpointSlotIds.Count);

            foreach (GameSaveSlotInfo slot in slots)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsCheckpointSlot(slot))
                    continue;

                entries.Add(await CreateEntryAsync(slot, cancellationToken));
            }

            return entries
                .OrderByDescending(entry => entry.UtcTimestamp)
                .ThenBy(entry => entry.SlotId, StringComparer.Ordinal)
                .ToArray();
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
            return sceneId == ExplorationSceneIds.World ? SceneNames.World : sceneId;
        }
    }
}
