
// Placement: Docs/Ru/02_ProjectStructure.md:192-202. Quote: "Содержит управление сценами."

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    private string currentScene;

    public string LoadedGameplayScene => currentScene;

    public UniTask SwitchTo(string sceneName)
    {
        return SwitchToAsync(sceneName, CancellationToken.None);
    }

    public async UniTask SwitchToAsync(string sceneName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (currentScene == sceneName)
            return;

        await LoadAdditiveAsync(sceneName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrEmpty(currentScene))
        {
            await UnloadAsync(currentScene, cancellationToken);
            currentScene = null;
            cancellationToken.ThrowIfCancellationRequested();
        }

        currentScene = sceneName;
    }

    public async UniTask ReloadAsync(string sceneName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrEmpty(currentScene))
        {
            await UnloadAsync(currentScene, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        await LoadAdditiveAsync(sceneName, cancellationToken);
        currentScene = sceneName;
    }

    public UniTask LoadAdditive(string sceneName)
    {
        return LoadAdditiveAsync(sceneName, CancellationToken.None);
    }

    public async UniTask LoadAdditiveAsync(string sceneName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsLoaded(sceneName))
            return;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );

        await operation.ToUniTask(cancellationToken: cancellationToken);
    }

    public UniTask Unload(string sceneName)
    {
        return UnloadAsync(sceneName, CancellationToken.None);
    }

    private async UniTask UnloadAsync(string sceneName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsLoaded(sceneName))
            return;

        AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
        await operation.ToUniTask(cancellationToken: cancellationToken);
    }

    public bool IsLoaded(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).isLoaded;
    }

    public void SetCurrentScene(string sceneName)
    {
        currentScene = sceneName;
    }
}
