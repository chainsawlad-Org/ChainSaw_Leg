using System;
using Cysharp.Threading.Tasks;

public class ExplorationPhase : SceneGamePhase
{
    private string targetSceneName = SceneNames.World;

    protected override string SceneName => targetSceneName;

    public ExplorationPhase(ISceneLoader sceneLoader) : base(sceneLoader)
    { }

    public void SetTargetScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            throw new ArgumentException("Exploration target scene is required.", nameof(sceneName));

        targetSceneName = sceneName;
    }

    public override UniTask Exit()
    {
        targetSceneName = SceneNames.World;
        return base.Exit();
    }
}
