using UnityEngine;

public class UnityTimeScaleController : ITimeScaleController
{
    public float TimeScale
    {
        get => Time.timeScale;
        set => Time.timeScale = value;
    }
}
