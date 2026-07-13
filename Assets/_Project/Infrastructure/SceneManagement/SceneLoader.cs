using System.Collections.Generic;
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

        bool targetWasLoaded = IsLoaded(sceneName);
        List<AudioListener> suspendedAudioListeners = SuspendAudioListeners(currentScene);

        try
        {
            await LoadAdditiveOperationAsync(sceneName);
        }
        catch
        {
            RestoreAudioListeners(suspendedAudioListeners);
            throw;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            if (!targetWasLoaded)
                await UnloadOperationAsync(sceneName);

            RestoreAudioListeners(suspendedAudioListeners);
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (!string.IsNullOrEmpty(currentScene))
            await UnloadOperationAsync(currentScene);

        currentScene = sceneName;
    }

    public async UniTask ReloadAsync(string sceneName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrEmpty(currentScene))
            await UnloadOperationAsync(currentScene);

        await LoadAdditiveOperationAsync(sceneName);
        currentScene = sceneName;
    }

    public UniTask LoadAdditive(string sceneName)
    {
        return LoadAdditiveAsync(sceneName, CancellationToken.None);
    }

    public async UniTask LoadAdditiveAsync(string sceneName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await LoadAdditiveOperationAsync(sceneName);
    }

    public UniTask Unload(string sceneName)
    {
        return UnloadAsync(sceneName, CancellationToken.None);
    }

    private async UniTask UnloadAsync(string sceneName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await UnloadOperationAsync(sceneName);
    }

    private async UniTask LoadAdditiveOperationAsync(string sceneName)
    {
        if (IsLoaded(sceneName))
            return;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive);
        await operation.ToUniTask();
    }

    private async UniTask UnloadOperationAsync(string sceneName)
    {
        if (!IsLoaded(sceneName))
            return;

        AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
        await operation.ToUniTask();
    }

    private static List<AudioListener> SuspendAudioListeners(string sceneName)
    {
        var suspendedListeners = new List<AudioListener>();

        if (string.IsNullOrEmpty(sceneName))
            return suspendedListeners;

        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
            return suspendedListeners;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (AudioListener listener in root.GetComponentsInChildren<AudioListener>(true))
            {
                if (!listener.enabled)
                    continue;

                listener.enabled = false;
                suspendedListeners.Add(listener);
            }
        }

        return suspendedListeners;
    }

    private static void RestoreAudioListeners(IEnumerable<AudioListener> listeners)
    {
        foreach (AudioListener listener in listeners)
        {
            if (listener != null)
                listener.enabled = true;
        }
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
