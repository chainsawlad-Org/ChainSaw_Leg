using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MainMenuUI : MonoBehaviour
{
    [Inject] private GameStateMachine gameStateMachine;
    [Inject] private IRuntimeErrorLogger runtimeErrorLogger;

    public void StartGame()
    {
        StartGameAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private async UniTask StartGameAsync(System.Threading.CancellationToken cancellationToken)
    {
        try
        {
            await gameStateMachine.ReplaceMainAsync<ExplorationPhase>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            runtimeErrorLogger.LogException(exception, nameof(MainMenuUI));
        }
    }
}
