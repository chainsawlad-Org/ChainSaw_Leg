public class GamePauseService
{
    private readonly ITimeScaleController timeScaleController;

    private int activePauseCount;
    private float previousTimeScale = 1f;

    public GamePauseService(ITimeScaleController timeScaleController)
    {
        this.timeScaleController = timeScaleController;
    }

    public bool IsPaused => activePauseCount > 0;
    public int ActivePauseCount => activePauseCount;

    public void AcquirePause()
    {
        if (activePauseCount == 0)
        {
            float currentTimeScale = timeScaleController.TimeScale;
            previousTimeScale = currentTimeScale > 0f ? currentTimeScale : 1f;
            timeScaleController.TimeScale = 0f;
        }

        activePauseCount++;
    }

    public void ReleasePause()
    {
        if (activePauseCount == 0)
            return;

        activePauseCount--;

        if (activePauseCount > 0)
            return;

        timeScaleController.TimeScale = previousTimeScale;
    }

    public void Reset()
    {
        float currentTimeScale = timeScaleController.TimeScale;

        activePauseCount = 0;

        if (currentTimeScale <= 0f)
            timeScaleController.TimeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

        previousTimeScale = timeScaleController.TimeScale;
    }
}
