using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using NUnit.Framework;

public sealed class ExplorationSaveTests
{
    [Test]
    public void ContributorCapturesSceneCheckpointAndPlayerPosition()
    {
        var player = new FakePlayerPositionProvider(12.5f, -4.25f);
        var context = new FakeSaveContextProvider("world", "checkpoint_gate");
        var contributor = new ExplorationSaveContributor(player, context);

        var saveData = (ExplorationSaveData)contributor.CaptureSaveData();

        Assert.That(saveData.SceneId, Is.EqualTo("world"));
        Assert.That(saveData.CheckpointId, Is.EqualTo("checkpoint_gate"));
        Assert.That(saveData.PositionX, Is.EqualTo(12.5f));
        Assert.That(saveData.PositionY, Is.EqualTo(-4.25f));
    }

    [Test]
    public void ContributorRejectsUnavailablePlayer()
    {
        var player = new FakePlayerPositionProvider(0f, 0f) { IsPlayerAvailable = false };
        var context = new FakeSaveContextProvider("world", null);
        var contributor = new ExplorationSaveContributor(player, context);

        Assert.Throws<GameSaveValidationException>(() => contributor.CaptureSaveData());
    }

    [Test]
    public void RestorerAppliesContextAndPosition()
    {
        var target = new FakeRestorationTarget();
        var context = new ExplorationSaveContextService();
        var restorer = new ExplorationSaveRestorer(target, context);

        restorer.RestoreSaveData(new ExplorationSaveData
        {
            SceneId = "world",
            CheckpointId = "checkpoint_shop",
            PositionX = 8f,
            PositionY = 3f
        });

        Assert.That(context.SceneId, Is.EqualTo("world"));
        Assert.That(context.CheckpointId, Is.EqualTo("checkpoint_shop"));
        Assert.That(target.PositionX, Is.EqualTo(8f));
        Assert.That(target.PositionY, Is.EqualTo(3f));
        Assert.That(target.RestoreCount, Is.EqualTo(1));
    }

    [Test]
    public void PendingRestoreIsClearedAfterUse()
    {
        var pending = new GameSavePendingRestoreService();
        var saveData = new GameSaveData();

        pending.SetPending(saveData);

        Assert.That(pending.HasPendingRestore, Is.True);
        Assert.That(pending.GetPending(), Is.SameAs(saveData));

        pending.Clear();

        Assert.That(pending.HasPendingRestore, Is.False);
        Assert.Throws<GameSaveValidationException>(() => pending.GetPending());
    }

    [Test]
    public void StalePlayerCannotUnregisterNewPlayer()
    {
        var registry = new ExplorationPlayerRegistry();
        var oldPlayer = new FakePlayerState(1f, 2f);
        var newPlayer = new FakePlayerState(10f, 20f);

        registry.Register(oldPlayer, oldPlayer);
        int oldVersion = registry.RegistrationVersion;
        registry.Register(newPlayer, newPlayer);
        registry.Unregister(oldPlayer, oldPlayer);
        registry.RestorePosition(30f, 40f);

        Assert.That(registry.RegistrationVersion, Is.GreaterThan(oldVersion));
        Assert.That(registry.PositionX, Is.EqualTo(30f));
        Assert.That(registry.PositionY, Is.EqualTo(40f));
        Assert.That(newPlayer.RestoreCount, Is.EqualTo(1));
    }

    private sealed class FakePlayerPositionProvider : IPlayerPositionProvider
    {
        public FakePlayerPositionProvider(float positionX, float positionY)
        {
            PositionX = positionX;
            PositionY = positionY;
        }

        public bool IsPlayerAvailable { get; set; } = true;
        public float PositionX { get; }
        public float PositionY { get; }
    }

    private sealed class FakeSaveContextProvider : IExplorationSaveContextProvider
    {
        public FakeSaveContextProvider(string sceneId, string checkpointId)
        {
            SceneId = sceneId;
            CheckpointId = checkpointId;
        }

        public string SceneId { get; }
        public string CheckpointId { get; }
    }

    private sealed class FakeRestorationTarget : IPlayerPositionRestorationTarget
    {
        public bool IsPlayerAvailable => true;
        public float PositionX { get; private set; }
        public float PositionY { get; private set; }
        public int RestoreCount { get; private set; }

        public void RestorePosition(float positionX, float positionY)
        {
            PositionX = positionX;
            PositionY = positionY;
            RestoreCount++;
        }
    }

    private sealed class FakePlayerState : IPlayerPositionProvider, IPlayerPositionRestorationTarget
    {
        public FakePlayerState(float positionX, float positionY)
        {
            PositionX = positionX;
            PositionY = positionY;
        }

        public bool IsPlayerAvailable => true;
        public float PositionX { get; private set; }
        public float PositionY { get; private set; }
        public int RestoreCount { get; private set; }

        public void RestorePosition(float positionX, float positionY)
        {
            PositionX = positionX;
            PositionY = positionY;
            RestoreCount++;
        }
    }
}
