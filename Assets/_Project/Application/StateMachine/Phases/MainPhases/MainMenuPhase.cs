
// Placement: Docs/Ru/02_ProjectStructure.md:98-116. Quote: "StateMachine знает только о жизненном цикле фаз."

public class MainMenuPhase : SceneGamePhase
{
    protected override string SceneName => SceneNames.MainMenu;
    public override bool AllowsGameplayInput => false;

    public MainMenuPhase(ISceneLoader sceneLoader) : base(sceneLoader)
    { }
}
