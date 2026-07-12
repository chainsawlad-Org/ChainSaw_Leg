// Placement: Docs/Ru/02_ProjectStructure.md:172-176. Quote: "Любой код, работающий непосредственно с API Unity, должен находиться здесь."

public interface ITimeScaleController
{
    float TimeScale { get; set; }
}
