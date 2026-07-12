using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChainSawLeg.Core.SaveSystem;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

public class FileGameSaveStorageProviderTests
{
    private string temporaryDirectory;
    private OdinGameSaveSerializer serializer;
    private FileGameSaveStorageProvider storageProvider;

    [SetUp]
    public void SetUp()
    {
        temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "ChainSawLeg.GameSaveTests",
            Guid.NewGuid().ToString("N"));
        serializer = new OdinGameSaveSerializer();
        storageProvider = new FileGameSaveStorageProvider(serializer, temporaryDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(temporaryDirectory))
            Directory.Delete(temporaryDirectory, true);
    }

    [Test]
    public async Task WriteCreatesDirectoryAndSafeSlotFile()
    {
        GameSaveRequest request = CreateRequest(GameSaveKind.Checkpoint, "checkpoint_0");

        await WriteSaveAsync(request, Utc(10));

        Assert.That(File.Exists(Path.Combine(temporaryDirectory, "checkpoint_0.save")), Is.True);
        Assert.That(Directory.GetFiles(temporaryDirectory, "*.tmp"), Is.Empty);
        Assert.That(Directory.GetFiles(temporaryDirectory, "*.backup"), Is.Empty);
        Assert.That(await storageProvider.SlotExistsAsync(request, CancellationToken.None), Is.True);
    }

    [Test]
    public async Task ReadReturnsWrittenBytes()
    {
        GameSaveRequest request = CreateRequest(GameSaveKind.Auto, "auto_0");
        byte[] expected = CreateSerializedSave(request, Utc(11), "build-read");

        await storageProvider.WriteAsync(request, expected, CancellationToken.None);
        byte[] actual = await storageProvider.ReadAsync(request, CancellationToken.None);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task OverwriteReplacesExistingSlot()
    {
        GameSaveRequest request = CreateRequest(GameSaveKind.Manual, "manual_0");

        await WriteSaveAsync(request, Utc(12), "build-old");
        await WriteSaveAsync(request, Utc(13), "build-new");
        GameSaveMetadata metadata = await storageProvider.ReadMetadataAsync(
            request,
            CancellationToken.None);

        Assert.That(metadata.BuildNumber, Is.EqualTo("build-new"));
        Assert.That(metadata.UtcTimestamp, Is.EqualTo(Utc(13)));
        Assert.That(Directory.GetFiles(temporaryDirectory, "*.tmp"), Is.Empty);
        Assert.That(Directory.GetFiles(temporaryDirectory, "*.backup"), Is.Empty);
    }

    [Test]
    public async Task ListReturnsExistingSlotsAndMetadata()
    {
        await WriteSaveAsync(CreateRequest(GameSaveKind.Manual, "manual_0"), Utc(10));
        await WriteSaveAsync(CreateRequest(GameSaveKind.Auto, "auto_0"), Utc(11));

        var slots = await storageProvider.ListSlotsAsync(CancellationToken.None);

        Assert.That(slots.Select(slot => slot.SlotId), Is.EquivalentTo(new[] { "manual_0", "auto_0" }));
        Assert.That(slots.All(slot => slot.Metadata != null && !slot.IsCorrupted), Is.True);
    }

    [Test]
    public async Task DeleteRemovesSlotAndExistenceState()
    {
        GameSaveRequest request = CreateRequest(GameSaveKind.Manual, "manual_1");
        await WriteSaveAsync(request, Utc(10));

        await storageProvider.DeleteSlotAsync(request, CancellationToken.None);

        Assert.That(await storageProvider.SlotExistsAsync(request, CancellationToken.None), Is.False);
        Assert.ThrowsAsync<GameSaveStorageException>(async () =>
            await storageProvider.ReadAsync(request, CancellationToken.None).AsTask());
    }

    [Test]
    public async Task CorruptedFileIsReportedWithoutBreakingSlotList()
    {
        Directory.CreateDirectory(temporaryDirectory);
        File.WriteAllBytes(Path.Combine(temporaryDirectory, "manual_0.save"), new byte[] { 1, 2, 3 });
        GameSaveRequest request = CreateRequest(GameSaveKind.Manual, "manual_0");

        Assert.ThrowsAsync<CorruptedGameSaveException>(async () =>
            await storageProvider.ReadAsync(request, CancellationToken.None).AsTask());

        var slots = await storageProvider.ListSlotsAsync(CancellationToken.None);

        Assert.That(slots, Has.Count.EqualTo(1));
        Assert.That(slots[0].SlotId, Is.EqualTo("manual_0"));
        Assert.That(slots[0].IsCorrupted, Is.True);
        Assert.That(slots[0].Metadata, Is.Null);
    }

    [TestCase("../manual_0")]
    [TestCase("/tmp/manual_0")]
    [TestCase("manual/0")]
    [TestCase("manual:0")]
    [TestCase("")]
    public void InvalidSlotIdIsRejected(string slotId)
    {
        GameSaveRequest request = CreateRequest(GameSaveKind.Manual, slotId);
        byte[] data = CreateSerializedSave(request, Utc(10));

        Assert.ThrowsAsync<GameSaveValidationException>(async () =>
            await storageProvider.WriteAsync(request, data, CancellationToken.None).AsTask());
    }

    [Test]
    public async Task ListIsSortedByTimestampDescendingWithCorruptedSlotsLast()
    {
        await WriteSaveAsync(CreateRequest(GameSaveKind.Manual, "manual_0"), Utc(10));
        await WriteSaveAsync(CreateRequest(GameSaveKind.Auto, "auto_0"), Utc(12));
        await WriteSaveAsync(CreateRequest(GameSaveKind.Checkpoint, "checkpoint_0"), Utc(11));
        File.WriteAllBytes(Path.Combine(temporaryDirectory, "manual_1.save"), Array.Empty<byte>());

        var slots = await storageProvider.ListSlotsAsync(CancellationToken.None);

        Assert.That(
            slots.Select(slot => slot.SlotId),
            Is.EqualTo(new[] { "auto_0", "checkpoint_0", "manual_0", "manual_1" }));
    }

    [Test]
    public async Task CheckpointRotationUsesFreeSlotsThenOldestSlot()
    {
        var rotationService = new CheckpointGameSaveSlotRotationService(storageProvider);

        Assert.That(
            await rotationService.GetNextSlotIdAsync(CancellationToken.None),
            Is.EqualTo("checkpoint_0"));

        await WriteSaveAsync(CreateRequest(GameSaveKind.Checkpoint, "checkpoint_0"), Utc(10));
        Assert.That(
            await rotationService.GetNextSlotIdAsync(CancellationToken.None),
            Is.EqualTo("checkpoint_1"));

        await WriteSaveAsync(CreateRequest(GameSaveKind.Checkpoint, "checkpoint_1"), Utc(12));
        Assert.That(
            await rotationService.GetNextSlotIdAsync(CancellationToken.None),
            Is.EqualTo("checkpoint_2"));

        await WriteSaveAsync(CreateRequest(GameSaveKind.Checkpoint, "checkpoint_2"), Utc(11));
        Assert.That(
            await rotationService.GetNextSlotIdAsync(CancellationToken.None),
            Is.EqualTo("checkpoint_0"));
    }

    private async UniTask WriteSaveAsync(
        GameSaveRequest request,
        DateTime timestamp,
        string buildNumber = "build-1")
    {
        byte[] data = CreateSerializedSave(request, timestamp, buildNumber);
        await storageProvider.WriteAsync(request, data, CancellationToken.None);
    }

    private byte[] CreateSerializedSave(
        GameSaveRequest request,
        DateTime timestamp,
        string buildNumber = "build-1")
    {
        var saveData = new GameSaveData
        {
            Metadata = GameSaveMetadata.Create(
                request,
                GameSaveData.CurrentFormatVersion,
                timestamp,
                buildNumber,
                "profile-a")
        };

        return serializer.Serialize(saveData);
    }

    private static GameSaveRequest CreateRequest(GameSaveKind kind, string slotId)
    {
        return new GameSaveRequest(kind, slotId);
    }

    private static DateTime Utc(int hour)
    {
        return new DateTime(2026, 7, 12, hour, 0, 0, DateTimeKind.Utc);
    }
}
