// Placement: Docs/Ru/02_ProjectStructure.md:172-176. Quote: "Любой код, работающий непосредственно с API Unity, должен находиться здесь."

using UnityEngine;

public class UnityTimeScaleController : ITimeScaleController
{
    public float TimeScale
    {
        get => Time.timeScale;
        set => Time.timeScale = value;
    }
}
