using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExplorationPhase : IGamePhase
{
    private readonly ISceneLoader sceneLoader;

    public ExplorationPhase(ISceneLoader sceneLoader)
    {
        this.sceneLoader = sceneLoader;
    }

    public async UniTask Enter()
    {
        Debug.Log("ENTER WORLD");

        await sceneLoader.Load("SC_World");
    }

    public async UniTask Exit()
    {
        Debug.Log("EXIT WORLD");

        await sceneLoader.Unload("SC_World");
    }
}

