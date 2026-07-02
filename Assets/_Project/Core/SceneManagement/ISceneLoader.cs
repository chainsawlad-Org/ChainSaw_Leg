using Cysharp.Threading.Tasks;

public interface ISceneLoader
{
    string LoadedGameplayScene { get; }
    UniTask SwitchTo(string sceneName);
    UniTask LoadAdditive(string sceneName);
    UniTask Unload(string sceneName);
    bool IsLoaded(string sceneName);
    void SetCurrentScene(string sceneName);
}
