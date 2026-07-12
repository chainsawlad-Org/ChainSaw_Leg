using System.Threading;
using Cysharp.Threading.Tasks;

public interface ISceneLoader
{
    string LoadedGameplayScene { get; }
    UniTask SwitchTo(string sceneName);
    UniTask SwitchToAsync(string sceneName, CancellationToken cancellationToken);
    UniTask ReloadAsync(string sceneName, CancellationToken cancellationToken);
    UniTask LoadAdditive(string sceneName);
    UniTask LoadAdditiveAsync(string sceneName, CancellationToken cancellationToken);
    UniTask Unload(string sceneName);
    bool IsLoaded(string sceneName);
    void SetCurrentScene(string sceneName);
}
