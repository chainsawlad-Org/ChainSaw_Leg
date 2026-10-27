using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Application
{
    public abstract class GamePhase : IGamePhase
    {
        public abstract UniTask Enter();

        public abstract UniTask Exit();
    }
}
