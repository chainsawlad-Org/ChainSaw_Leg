public class MainMenuPhase : SceneGamePhase
{
    protected override string SceneName => SceneNames.MainMenu;
    public override bool AllowsGameplayInput => false;

    public MainMenuPhase(ISceneLoader sceneLoader) : base(sceneLoader)
    { }
}
