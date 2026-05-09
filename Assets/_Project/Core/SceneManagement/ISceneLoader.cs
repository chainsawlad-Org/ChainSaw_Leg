using Cysharp.Threading.Tasks;

public interface ISceneLoader
{
    UniTask Load(string sceneName);
    UniTask LoadAdditive(string sceneName);
    UniTask Unload(string sceneName);
}
