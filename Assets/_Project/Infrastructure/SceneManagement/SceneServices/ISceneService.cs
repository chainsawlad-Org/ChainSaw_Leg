
using Cysharp.Threading.Tasks;

public interface ISceneService
{
    UniTask Initialize();
    UniTask Dispose();
}
