
public class BattlePhase : SceneGamePhase
{
    protected override string SceneName => SceneNames.Battle;

    public BattlePhase(ISceneLoader sceneLoader) : base(sceneLoader) { }
}