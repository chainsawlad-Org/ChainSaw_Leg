using NUnit.Framework;

public class GameplayInputBlockServiceTests
{
    [Test]
    public void DialogueStyleBlockKeepsSubmitAvailable()
    {
        var service = new GameplayInputBlockService();

        service.AcquireBlock(InputBlockChannels.Move | InputBlockChannels.Dash | InputBlockChannels.Interact);

        Assert.That(service.IsChannelBlocked(InputBlockChannels.Move), Is.True);
        Assert.That(service.IsChannelBlocked(InputBlockChannels.Dash), Is.True);
        Assert.That(service.IsChannelBlocked(InputBlockChannels.Interact), Is.True);
        Assert.That(service.IsChannelBlocked(InputBlockChannels.Submit), Is.False);
    }

    [Test]
    public void ReleasingOneOfTwoMatchingBlocksKeepsChannelBlocked()
    {
        var service = new GameplayInputBlockService();

        service.AcquireBlock(InputBlockChannels.Submit);
        service.AcquireBlock(InputBlockChannels.Submit);
        service.ReleaseBlock(InputBlockChannels.Submit);

        Assert.That(service.IsChannelBlocked(InputBlockChannels.Submit), Is.True);
    }

    [Test]
    public void ReleasingLastBlockUnblocksChannel()
    {
        var service = new GameplayInputBlockService();

        service.AcquireBlock(InputBlockChannels.Gameplay);
        service.ReleaseBlock(InputBlockChannels.Gameplay);

        Assert.That(service.IsChannelBlocked(InputBlockChannels.Gameplay), Is.False);
    }

    [Test]
    public void ExtraReleaseDoesNotGoBelowZero()
    {
        var service = new GameplayInputBlockService();

        service.ReleaseBlock(InputBlockChannels.Gameplay);
        service.AcquireBlock(InputBlockChannels.Move);
        service.ReleaseBlock(InputBlockChannels.Move);
        service.ReleaseBlock(InputBlockChannels.Move);

        Assert.That(service.IsChannelBlocked(InputBlockChannels.Move), Is.False);
        Assert.That(service.ActiveBlockCount, Is.EqualTo(0));
    }
}
