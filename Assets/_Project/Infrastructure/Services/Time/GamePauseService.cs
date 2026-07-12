// Placement: Docs/Ru/02_ProjectStructure.md:172-176. Quote: "Любой код, работающий непосредственно с API Unity, должен находиться здесь."

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
            previousTimeScale = timeScaleController.TimeScale;
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
        if (activePauseCount == 0)
            return;

        activePauseCount = 0;
        timeScaleController.TimeScale = previousTimeScale;
    }
}
