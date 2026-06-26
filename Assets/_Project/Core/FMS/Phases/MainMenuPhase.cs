using Cysharp.Threading.Tasks;
using UnityEngine;

public class MainMenuPhase : IGamePhase
{
    private readonly ISceneLoader sceneLoader;

    public MainMenuPhase(ISceneLoader sceneLoader)
    {
        this.sceneLoader = sceneLoader;
    }
    public async UniTask Enter()
    {

        await sceneLoader.SwitchTo("SC_MainMenu");
    }

    public UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
