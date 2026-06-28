
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    private string currentScene;

    public string LoadedGameplayScene => currentScene;

    public async UniTask SwitchTo(string sceneName)
    {
        if (currentScene == sceneName)
            return;

        await LoadAdditive(sceneName);

        if (!string.IsNullOrEmpty(currentScene))
        {
            await Unload(currentScene);
        }

        currentScene = sceneName;
    }

    public async UniTask LoadAdditive(string sceneName)
    {
        if (IsLoaded(sceneName))
            return;

        await SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );
    }

    public async UniTask Unload(string sceneName)
    {
        if (!IsLoaded(sceneName))
            return;

        await SceneManager.UnloadSceneAsync(sceneName);
    }

    public bool IsLoaded(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).isLoaded;
    }


}
