using UnityEngine;
using Zenject;

public class MainMenuUI : MonoBehaviour
{
    [Inject] private GameStateMachine fms;
    [Inject] private ISceneLoader sceneLoader;

    public async void StartGame()
    {
        await fms.Push(
            new ExplorationPhase(sceneLoader));
    }
}
