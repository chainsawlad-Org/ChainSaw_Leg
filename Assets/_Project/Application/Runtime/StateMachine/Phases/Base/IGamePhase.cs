using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Application
{
    public interface IGamePhase
    {
        UniTask Enter();
        UniTask Exit();
    }
}
