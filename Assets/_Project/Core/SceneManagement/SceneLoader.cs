
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{

    private string currentScene;

    public async UniTask LoadAdditive(string sceneName)
    {
        await SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );
    }

    public async UniTask Unload(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
            return;

        await SceneManager.UnloadSceneAsync(sceneName);
    }

    public async UniTask SwitchTo(string sceneName)
    {
        if (currentScene == sceneName)
            return;

        if (!string.IsNullOrEmpty(currentScene))
        {
            await Unload(currentScene);
        }

        await LoadAdditive(sceneName);

        currentScene = sceneName;
    }
}
