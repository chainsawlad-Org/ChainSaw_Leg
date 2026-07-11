using NUnit.Framework;

public class GamePauseServiceTests
{
    [Test]
    public void FirstPauseSetsTimeScaleToZero()
    {
        var timeScaleController = new FakeTimeScaleController(0.75f);
        var gamePauseService = new GamePauseService(timeScaleController);

        gamePauseService.AcquirePause();

        Assert.That(timeScaleController.TimeScale, Is.EqualTo(0f));
        Assert.That(gamePauseService.ActivePauseCount, Is.EqualTo(1));
    }

    [Test]
    public void SecondPauseKeepsGamePaused()
    {
        var timeScaleController = new FakeTimeScaleController(1f);
        var gamePauseService = new GamePauseService(timeScaleController);

        gamePauseService.AcquirePause();
        gamePauseService.AcquirePause();

        Assert.That(timeScaleController.TimeScale, Is.EqualTo(0f));
        Assert.That(gamePauseService.ActivePauseCount, Is.EqualTo(2));
    }

    [Test]
    public void ReleasingOneOfTwoPausesDoesNotResumeGame()
    {
        var timeScaleController = new FakeTimeScaleController(1f);
        var gamePauseService = new GamePauseService(timeScaleController);

        gamePauseService.AcquirePause();
        gamePauseService.AcquirePause();
        gamePauseService.ReleasePause();

        Assert.That(timeScaleController.TimeScale, Is.EqualTo(0f));
        Assert.That(gamePauseService.ActivePauseCount, Is.EqualTo(1));
    }

    [Test]
    public void ReleasingLastPauseRestoresOriginalTimeScale()
    {
        var timeScaleController = new FakeTimeScaleController(0.5f);
        var gamePauseService = new GamePauseService(timeScaleController);

        gamePauseService.AcquirePause();
        gamePauseService.AcquirePause();
        gamePauseService.ReleasePause();
        gamePauseService.ReleasePause();

        Assert.That(timeScaleController.TimeScale, Is.EqualTo(0.5f));
        Assert.That(gamePauseService.ActivePauseCount, Is.EqualTo(0));
    }

    [Test]
    public void ExtraReleaseDoesNotMoveCounterBelowZero()
    {
        var timeScaleController = new FakeTimeScaleController(1f);
        var gamePauseService = new GamePauseService(timeScaleController);

        gamePauseService.ReleasePause();
        gamePauseService.AcquirePause();
        gamePauseService.ReleasePause();
        gamePauseService.ReleasePause();

        Assert.That(timeScaleController.TimeScale, Is.EqualTo(1f));
        Assert.That(gamePauseService.ActivePauseCount, Is.EqualTo(0));
    }

    private sealed class FakeTimeScaleController : ITimeScaleController
    {
        public FakeTimeScaleController(float initialTimeScale)
        {
            TimeScale = initialTimeScale;
        }

        public float TimeScale { get; set; }
    }
}
