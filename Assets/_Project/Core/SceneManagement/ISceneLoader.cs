using Cysharp.Threading.Tasks;

public interface ISceneLoader
{
    UniTask LoadAdditive(string sceneName);
    UniTask Unload(string sceneName);
    UniTask SwitchTo(string sceneName);
}
