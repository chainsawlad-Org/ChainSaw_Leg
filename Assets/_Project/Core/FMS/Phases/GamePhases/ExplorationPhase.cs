
public class ExplorationPhase : SceneGamePhase
{
    protected override string SceneName => SceneNames.World;

    public ExplorationPhase(ISceneLoader sceneLoader) : base(sceneLoader)
    { }
}

