using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChainSawLeg.Core.SaveSystem;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameSaveSystemTests
{
    [Test]
    public void CreateMetadataCopiesRequestAndRuntimeValues()
    {
        var request = new GameSaveRequest(GameSaveKind.Manual, "slot-a");
        DateTime timestamp = new DateTime(2026, 7, 12, 10, 30, 0, DateTimeKind.Utc);

        GameSaveMetadata metadata = GameSaveMetadata.Create(
            request,
            GameSaveData.CurrentFormatVersion,
            timestamp,
            "build-42",
            "profile-a");

        Assert.That(metadata.FormatVersion, Is.EqualTo(GameSaveData.CurrentFormatVersion));
        Assert.That(metadata.UtcTimestamp, Is.EqualTo(timestamp));
        Assert.That(metadata.UtcTimestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
        Assert.That(metadata.BuildNumber, Is.EqualTo("build-42"));
        Assert.That(metadata.ProfileId, Is.EqualTo("profile-a"));
        Assert.That(metadata.Kind, Is.EqualTo(GameSaveKind.Manual));
        Assert.That(metadata.SlotId, Is.EqualTo("slot-a"));
    }

    [Test]
    public void EmptySlotIdIsRejected()
    {
        var validationService = new GameSaveValidationService();
        var request = new GameSaveRequest(GameSaveKind.Manual, " ");

        Assert.Throws<GameSaveValidationException>(() => validationService.ValidateRequest(request));
    }

    [Test]
    public void UnknownFormatVersionIsRejected()
    {
        var migrationService = new GameSaveMigrationService(new List<IGameSaveMigrationStep>());
        GameSaveData saveData = CreateSaveData(formatVersion: 999);

        Assert.Throws<UnknownGameSaveVersionException>(() => migrationService.Migrate(saveData));
    }

    [Test]
    public async Task SaveAndLoadRestoresTestDto()
    {
        var storageProvider = new InMemoryGameSaveStorageProvider();
        var contributor = new TestSaveParticipant(42);
        GameSaveCoordinator coordinator = CreateCoordinator(storageProvider, contributor, contributor);
        var request = new GameSaveRequest(GameSaveKind.Manual, "slot-round-trip");

        await coordinator.SaveAsync(request, "profile-a", "build-1", CancellationToken.None);
        contributor.RestoredValue = 0;
        await coordinator.LoadAsync(request, CancellationToken.None);

        Assert.That(contributor.RestoredValue, Is.EqualTo(42));
    }

    [Test]
    public void OdinRoundTripPreservesImmutableSaveProperties()
    {
        GameSaveData source = CreateSaveData(GameSaveData.CurrentFormatVersion);
        var serializer = new OdinGameSaveSerializer();

        GameSaveData restored = serializer.Deserialize<GameSaveData>(serializer.Serialize(source));

        Assert.That(restored.Metadata, Is.Not.Null);
        Assert.That(restored.Metadata.SlotId, Is.EqualTo("slot-version"));
        Assert.That(restored.Metadata.FormatVersion, Is.EqualTo(GameSaveData.CurrentFormatVersion));
        Assert.That(restored.Entries, Is.Not.Null);
    }

    [Test]
    public async Task CorruptedStoredDataThrowsSerializationError()
    {
        var storageProvider = new InMemoryGameSaveStorageProvider();
        GameSaveCoordinator coordinator = CreateCoordinator(storageProvider, null, null);
        var request = new GameSaveRequest(GameSaveKind.Auto, "slot-corrupted");
        storageProvider.Set(request, new byte[] { 0x01, 0x02, 0x03, 0x04 });
        LogAssert.Expect(LogType.Error, "Failed to enter node ''.");

        GameSaveSerializationException exception = null;

        try
        {
            await coordinator.LoadAsync(request, CancellationToken.None);
        }
        catch (GameSaveSerializationException caughtException)
        {
            exception = caughtException;
        }

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void OldFormatInvokesRegisteredMigration()
    {
        var migrationStep = new TestMigrationStep();
        var migrationService = new GameSaveMigrationService(
            new List<IGameSaveMigrationStep> { migrationStep });
        GameSaveData saveData = CreateSaveData(formatVersion: 0);

        GameSaveData migratedData = migrationService.Migrate(saveData);

        Assert.That(migrationStep.WasInvoked, Is.True);
        Assert.That(migratedData.Metadata.FormatVersion, Is.EqualTo(GameSaveData.CurrentFormatVersion));
    }

    private static GameSaveCoordinator CreateCoordinator(
        IGameSaveStorageProvider storageProvider,
        IGameSaveContributor contributor,
        IGameSaveRestorer restorer)
    {
        return new GameSaveCoordinator(
            new OdinGameSaveSerializer(),
            storageProvider,
            new GameSaveValidationService(),
            new GameSaveMigrationService(new List<IGameSaveMigrationStep>()),
            contributor == null
                ? new List<IGameSaveContributor>()
                : new List<IGameSaveContributor> { contributor },
            restorer == null
                ? new List<IGameSaveRestorer>()
                : new List<IGameSaveRestorer> { restorer });
    }

    private static GameSaveData CreateSaveData(int formatVersion)
    {
        var request = new GameSaveRequest(GameSaveKind.Manual, "slot-version");

        return new GameSaveData(
            GameSaveMetadata.Create(
                request,
                formatVersion,
                DateTime.UtcNow,
                "build-1",
                "profile-a"));
    }

    [Serializable]
    private sealed class TestSaveDto
    {
        public int Value;
    }

    private sealed class TestSaveParticipant : IGameSaveContributor, IGameSaveRestorer
    {
        private readonly int savedValue;

        public TestSaveParticipant(int savedValue)
        {
            this.savedValue = savedValue;
        }

        public string ContributorId => "test-participant";
        public Type SaveDataType => typeof(TestSaveDto);
        public int RestoredValue { get; set; }

        public object CaptureSaveData()
        {
            return new TestSaveDto { Value = savedValue };
        }

        public void RestoreSaveData(object saveData)
        {
            RestoredValue = ((TestSaveDto)saveData).Value;
        }
    }

    private sealed class TestMigrationStep : IGameSaveMigrationStep
    {
        public int SourceVersion => 0;
        public int TargetVersion => GameSaveData.CurrentFormatVersion;
        public bool WasInvoked { get; private set; }

        public GameSaveData Migrate(GameSaveData saveData)
        {
            WasInvoked = true;
            return saveData;
        }
    }

    private sealed class InMemoryGameSaveStorageProvider : IGameSaveStorageProvider
    {
        private readonly Dictionary<string, byte[]> entries = new();

        public UniTask WriteAsync(
            GameSaveRequest request,
            byte[] data,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            entries[BuildKey(request)] = (byte[])data.Clone();
            return UniTask.CompletedTask;
        }

        public UniTask<byte[]> ReadAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!entries.TryGetValue(BuildKey(request), out byte[] data))
                throw new GameSaveStorageException("Test game save was not found.");

            return UniTask.FromResult((byte[])data.Clone());
        }

        public UniTask<IReadOnlyList<GameSaveSlotInfo>> ListSlotsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return UniTask.FromResult<IReadOnlyList<GameSaveSlotInfo>>(Array.Empty<GameSaveSlotInfo>());
        }

        public UniTask<bool> SlotExistsAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return UniTask.FromResult(entries.ContainsKey(BuildKey(request)));
        }

        public UniTask DeleteSlotAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            entries.Remove(BuildKey(request));
            return UniTask.CompletedTask;
        }

        public UniTask<GameSaveMetadata> ReadMetadataAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] data = entries[BuildKey(request)];
            GameSaveData saveData = new OdinGameSaveSerializer().Deserialize<GameSaveData>(data);
            return UniTask.FromResult(saveData.Metadata);
        }

        public void Set(GameSaveRequest request, byte[] data)
        {
            entries[BuildKey(request)] = (byte[])data.Clone();
        }

        private static string BuildKey(GameSaveRequest request)
        {
            return $"{request.Kind}:{request.SlotId}:{request.CheckpointId}";
        }
    }
}
