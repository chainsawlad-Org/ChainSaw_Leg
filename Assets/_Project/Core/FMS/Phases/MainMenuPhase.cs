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
        Debug.Log("ENTER MAIN MENU");

        await sceneLoader.Load("SC_MainMenu");
    }

    public async UniTask Exit()
    {
        Debug.Log("EXIT MAIN MENU");

        await sceneLoader.Unload("SC_MainMenu");
    }
}
