using UnityEngine;
using Zenject;

public class MainMenuUI : MonoBehaviour
{
    [Inject] private GameStateMachine gameStateMachine;

    public async void StartGame()
    {
        await gameStateMachine.ReplaceMain<ExplorationPhase>();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
