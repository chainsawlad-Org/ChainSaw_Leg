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
        await sceneLoader.SwitchTo("SC_World");
    }

    public UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}

