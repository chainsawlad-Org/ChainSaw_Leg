using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

public sealed class ExplorationSaveCatalogServiceTests
{
    [Test]
    public async Task CatalogShowsOnlyCheckpointSlotsSortedNewestFirst()
    {
        var serializer = new OdinGameSaveSerializer();
        var storage = new CatalogStorageProvider(serializer);
        storage.Add(GameSaveKind.Checkpoint, "checkpoint_0", Utc(10), "checkpoint_old");
        storage.Add(GameSaveKind.Manual, "manual_0", Utc(12), "manual_place");
        storage.Add(GameSaveKind.Checkpoint, "checkpoint_1", Utc(11), "checkpoint_new");
        storage.AddCorrupted("checkpoint_2");

        var coordinator = new GameSaveCoordinator(
            serializer,
            storage,
            new GameSaveValidationService(),
            new GameSaveMigrationService(new List<IGameSaveMigrationStep>()),
            new List<IGameSaveContributor>(),
            new List<IGameSaveRestorer>());
        var catalog = new ExplorationSaveCatalogService(storage, coordinator);

        IReadOnlyList<GameSaveCatalogEntry> entries =
            await catalog.GetCheckpointEntriesAsync(CancellationToken.None);

        Assert.That(entries.Count, Is.EqualTo(3));
        Assert.That(entries[0].SlotId, Is.EqualTo("checkpoint_1"));
        Assert.That(entries[0].CheckpointId, Is.EqualTo("checkpoint_new"));
        Assert.That(entries[0].SceneName, Is.EqualTo(SceneNames.World));
        Assert.That(entries[1].SlotId, Is.EqualTo("checkpoint_0"));
        Assert.That(entries[2].SlotId, Is.EqualTo("checkpoint_2"));
        Assert.That(entries[2].IsLoadable, Is.False);
    }

    private static DateTime Utc(int hour)
    {
        return new DateTime(2026, 7, 12, hour, 0, 0, DateTimeKind.Utc);
    }

    private sealed class CatalogStorageProvider : IGameSaveStorageProvider
    {
        private readonly IGameSaveSerializer serializer;
        private readonly Dictionary<string, byte[]> dataBySlot = new();
        private readonly List<GameSaveSlotInfo> slots = new();

        public CatalogStorageProvider(IGameSaveSerializer serializer)
        {
            this.serializer = serializer;
        }

        public void Add(
            GameSaveKind kind,
            string slotId,
            DateTime timestamp,
            string checkpointId)
        {
            var request = new GameSaveRequest(kind, slotId, checkpointId);
            var metadata = GameSaveMetadata.Create(
                request,
                GameSaveData.CurrentFormatVersion,
                timestamp,
                "build-test",
                "profile-test");
            var saveData = new GameSaveData { Metadata = metadata };
            saveData.Entries.Add(new GameSaveEntry
            {
                ContributorId = ExplorationSaveContributor.Id,
                Payload = serializer.Serialize(
                    new ExplorationSaveData
                    {
                        SceneId = ExplorationSceneIds.World,
                        CheckpointId = checkpointId,
                        PositionX = 1f,
                        PositionY = 2f
                    },
                    typeof(ExplorationSaveData))
            });

            dataBySlot[slotId] = serializer.Serialize(saveData);
            slots.Add(new GameSaveSlotInfo
            {
                SlotId = slotId,
                Metadata = metadata,
                IsCorrupted = false
            });
        }

        public void AddCorrupted(string slotId)
        {
            slots.Add(new GameSaveSlotInfo
            {
                SlotId = slotId,
                Metadata = null,
                IsCorrupted = true
            });
        }

        public UniTask<byte[]> ReadAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            return UniTask.FromResult(dataBySlot[request.SlotId]);
        }

        public UniTask<IReadOnlyList<GameSaveSlotInfo>> ListSlotsAsync(
            CancellationToken cancellationToken)
        {
            return UniTask.FromResult<IReadOnlyList<GameSaveSlotInfo>>(slots);
        }

        public UniTask WriteAsync(
            GameSaveRequest request,
            byte[] data,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public UniTask<bool> SlotExistsAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public UniTask DeleteSlotAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public UniTask<GameSaveMetadata> ReadMetadataAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
