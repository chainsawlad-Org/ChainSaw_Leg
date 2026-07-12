
// Placement: Docs/Ru/02_ProjectStructure.md:98-116. Quote: "StateMachine знает только о жизненном цикле фаз."

public class BattlePhase : SceneGamePhase
{
    protected override string SceneName => SceneNames.Battle;

    public BattlePhase(ISceneLoader sceneLoader) : base(sceneLoader) { }
}
