
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    private string currentScene;

    private string CurrentScene => currentScene;

    public async UniTask LoadAdditive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (scene.isLoaded)
            return;

        await SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );
    }

    public async UniTask Unload(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
            return;

        await SceneManager.UnloadSceneAsync(sceneName);
    }

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
}
