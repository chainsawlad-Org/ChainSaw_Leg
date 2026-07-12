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
        Debug.Log("Exit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
