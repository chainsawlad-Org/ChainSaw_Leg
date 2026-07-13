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
    public async Task CatalogShowsOnlyCheckpointSlotsInFixedSlotOrder()
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
        Assert.That(entries[0].SlotId, Is.EqualTo("checkpoint_0"));
        Assert.That(entries[0].CheckpointId, Is.EqualTo("checkpoint_old"));
        Assert.That(entries[0].SceneName, Is.EqualTo(SceneNames.World));
        Assert.That(entries[1].SlotId, Is.EqualTo("checkpoint_1"));
        Assert.That(entries[1].CheckpointId, Is.EqualTo("checkpoint_new"));
        Assert.That(entries[2].SlotId, Is.EqualTo("checkpoint_2"));
        Assert.That(entries[2].IsLoadable, Is.False);
    }

    [Test]
    public async Task SaveMenuEntriesReturnAllTenSlotsInFixedOrderWithEmptyPlaceholders()
    {
        var serializer = new OdinGameSaveSerializer();
        var storage = new CatalogStorageProvider(serializer);
        storage.Add(GameSaveKind.Checkpoint, "checkpoint_0", Utc(10), "checkpoint_old");
        storage.Add(GameSaveKind.Checkpoint, "checkpoint_2", Utc(11), "checkpoint_new");
        storage.AddCorrupted("checkpoint_5");

        var coordinator = new GameSaveCoordinator(
            serializer,
            storage,
            new GameSaveValidationService(),
            new GameSaveMigrationService(new List<IGameSaveMigrationStep>()),
            new List<IGameSaveContributor>(),
            new List<IGameSaveRestorer>());
        var catalog = new ExplorationSaveCatalogService(storage, coordinator);

        IReadOnlyList<GameSaveCatalogEntry> entries =
            await catalog.GetCheckpointSaveMenuEntriesAsync(CancellationToken.None);

        Assert.That(entries.Count, Is.EqualTo(10));

        for (int index = 0; index < entries.Count; index++)
            Assert.That(entries[index].SlotId, Is.EqualTo($"checkpoint_{index}"));

        Assert.That(entries[0].IsEmpty, Is.False);
        Assert.That(entries[0].CheckpointId, Is.EqualTo("checkpoint_old"));
        Assert.That(entries[1].IsEmpty, Is.True);
        Assert.That(entries[1].IsLoadable, Is.False);
        Assert.That(entries[2].IsEmpty, Is.False);
        Assert.That(entries[2].CheckpointId, Is.EqualTo("checkpoint_new"));
        Assert.That(entries[5].IsEmpty, Is.False);
        Assert.That(entries[5].IsLoadable, Is.False);
        Assert.That(entries[9].IsEmpty, Is.True);
    }

    [Test]
    public async Task FullCatalogIncludesCheckpointAutoAndManualSlots()
    {
        var serializer = new OdinGameSaveSerializer();
        var storage = new CatalogStorageProvider(serializer);
        storage.Add(GameSaveKind.Checkpoint, "checkpoint_0", Utc(10), "checkpoint_place");
        storage.Add(GameSaveKind.Auto, "auto_0", Utc(11), "auto_place");
        storage.Add(GameSaveKind.Manual, "manual_0", Utc(12), "manual_place");

        var coordinator = new GameSaveCoordinator(
            serializer,
            storage,
            new GameSaveValidationService(),
            new GameSaveMigrationService(new List<IGameSaveMigrationStep>()),
            new List<IGameSaveContributor>(),
            new List<IGameSaveRestorer>());
        var catalog = new ExplorationSaveCatalogService(storage, coordinator);

        IReadOnlyList<GameSaveCatalogEntry> entries =
            await catalog.GetEntriesAsync(CancellationToken.None);

        Assert.That(entries.Count, Is.EqualTo(3));
        Assert.That(entries[0].Kind, Is.EqualTo(GameSaveKind.Checkpoint));
        Assert.That(entries[1].Kind, Is.EqualTo(GameSaveKind.Auto));
        Assert.That(entries[2].Kind, Is.EqualTo(GameSaveKind.Manual));
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
            var explorationData = new ExplorationSaveData(
                ExplorationSceneIds.World,
                checkpointId,
                1f,
                2f);
            var saveData = new GameSaveData(
                metadata,
                new[]
                {
                    new GameSaveEntry(
                        ExplorationSaveContributor.Id,
                        serializer.Serialize(explorationData, typeof(ExplorationSaveData)))
                });

            dataBySlot[slotId] = serializer.Serialize(saveData);
            slots.Add(new GameSaveSlotInfo(slotId, metadata, isCorrupted: false));
        }

        public void AddCorrupted(string slotId)
        {
            slots.Add(new GameSaveSlotInfo(slotId, metadata: null, isCorrupted: true));
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
