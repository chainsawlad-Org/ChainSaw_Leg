
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    public async UniTask Load(string sceneName)
    {
        await SceneManager.LoadSceneAsync(sceneName);
    }

    public async UniTask LoadAdditive(string sceneName)
    {
        await SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );
    }

    public async UniTask Unload(string sceneName)
    {
        await SceneManager.UnloadSceneAsync(sceneName);
    }
}
