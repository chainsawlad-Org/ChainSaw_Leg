using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

public sealed class ExplorationCheckpointSaveServiceTests
{
    [Test]
    public async Task CheckpointUsesRotatedSlotAndPreservesWorldCheckpointId()
    {
        var serializer = new CapturingSerializer();
        var storageProvider = new CapturingStorageProvider();
        var context = new ExplorationSaveContextService();
        var coordinator = new GameSaveCoordinator(
            serializer,
            storageProvider,
            new GameSaveValidationService(),
            new GameSaveMigrationService(new List<IGameSaveMigrationStep>()),
            new List<IGameSaveContributor> { new TestContributor() },
            new List<IGameSaveRestorer>());
        var service = new ExplorationCheckpointSaveService(
            coordinator,
            new CheckpointGameSaveSlotRotationService(storageProvider),
            context,
            new TestMetadataProvider());

        await service.SaveCheckpointAsync("world_gate", CancellationToken.None);

        Assert.That(storageProvider.LastRequest.Kind, Is.EqualTo(GameSaveKind.Checkpoint));
        Assert.That(storageProvider.LastRequest.SlotId, Is.EqualTo("checkpoint_0"));
        Assert.That(storageProvider.LastRequest.CheckpointId, Is.EqualTo("world_gate"));
        Assert.That(context.CheckpointId, Is.EqualTo("world_gate"));
        Assert.That(serializer.LastSnapshot.Metadata.ProfileId, Is.EqualTo("profile-test"));
        Assert.That(serializer.LastSnapshot.Metadata.BuildNumber, Is.EqualTo("build-test"));

        service.Dispose();
    }

    [Test]
    public async Task SaveCheckpointToSlotAsyncUsesExplicitSlotIdAndBypassesRotation()
    {
        var serializer = new CapturingSerializer();
        var storageProvider = new CapturingStorageProvider();
        var context = new ExplorationSaveContextService();
        var coordinator = new GameSaveCoordinator(
            serializer,
            storageProvider,
            new GameSaveValidationService(),
            new GameSaveMigrationService(new List<IGameSaveMigrationStep>()),
            new List<IGameSaveContributor> { new TestContributor() },
            new List<IGameSaveRestorer>());
        var service = new ExplorationCheckpointSaveService(
            coordinator,
            new CheckpointGameSaveSlotRotationService(storageProvider),
            context,
            new TestMetadataProvider());

        await service.SaveCheckpointToSlotAsync("checkpoint_7", "world_gate", CancellationToken.None);

        Assert.That(storageProvider.LastRequest.Kind, Is.EqualTo(GameSaveKind.Checkpoint));
        Assert.That(storageProvider.LastRequest.SlotId, Is.EqualTo("checkpoint_7"));
        Assert.That(storageProvider.LastRequest.CheckpointId, Is.EqualTo("world_gate"));
        Assert.That(context.CheckpointId, Is.EqualTo("world_gate"));

        service.Dispose();
    }

    private sealed class TestMetadataProvider : IGameSaveRuntimeMetadataProvider
    {
        public string ProfileId => "profile-test";
        public string BuildNumber => "build-test";
    }

    private sealed class TestContributor : IGameSaveContributor
    {
        public string ContributorId => "test";
        public Type SaveDataType => typeof(TestSaveData);
        public object CaptureSaveData() => new TestSaveData { Value = 1 };
    }

    [Serializable]
    private sealed class TestSaveData
    {
        public int Value;
    }

    private sealed class CapturingSerializer : IGameSaveSerializer
    {
        public GameSaveData LastSnapshot { get; private set; }

        public byte[] Serialize<T>(T value)
        {
            LastSnapshot = value as GameSaveData;
            return new byte[] { 1 };
        }

        public byte[] Serialize(object value, Type expectedType)
        {
            return new byte[] { 1 };
        }

        public T Deserialize<T>(byte[] data)
        {
            throw new NotSupportedException();
        }

        public object Deserialize(byte[] data, Type expectedType)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class CapturingStorageProvider : IGameSaveStorageProvider
    {
        public GameSaveRequest LastRequest { get; private set; }

        public UniTask WriteAsync(
            GameSaveRequest request,
            byte[] data,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return UniTask.CompletedTask;
        }

        public UniTask<byte[]> ReadAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public UniTask<IReadOnlyList<GameSaveSlotInfo>> ListSlotsAsync(
            CancellationToken cancellationToken)
        {
            IReadOnlyList<GameSaveSlotInfo> slots = Array.Empty<GameSaveSlotInfo>();
            return UniTask.FromResult(slots);
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
