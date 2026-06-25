using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Bootstrapper : MonoBehaviour
{
    [Inject] private GameStateMachine gameStateMachine;
    [Inject] private ISceneLoader sceneLoader;

    private async void Start()
    {
        await StartGame();
    }

    private async UniTask StartGame()
    {
        await sceneLoader.LoadAdditive("Presistent");

        MainMenuPhase mainMenuPhase = new MainMenuPhase(sceneLoader);

        await gameStateMachine.Push(mainMenuPhase);
    }
}
