using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Game.Shared
{
    public interface IApplicationService
    {
        UniTask Initialize();
        UniTask Dispose();
    }
}
