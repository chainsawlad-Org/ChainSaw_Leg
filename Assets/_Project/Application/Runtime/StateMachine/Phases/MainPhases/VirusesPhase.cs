using System;
using Cysharp.Threading.Tasks;

public class VirusesPhase : SceneGamePhase
{
    private string targetSceneName = SceneNames.Viruses;

    protected override string SceneName => targetSceneName;

    public VirusesPhase(ISceneLoader sceneLoader) : base(sceneLoader)
    { }

    public void SetTargetScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            throw new ArgumentException("Exploration target scene is required.", nameof(sceneName));

        targetSceneName = sceneName;
    }

    public override UniTask Exit()
    {
        targetSceneName = SceneNames.Viruses;
        return base.Exit();
    }
}


