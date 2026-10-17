using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Game.Shared
{
    public abstract class ApplicationServiceBase : IApplicationService
    {
        public virtual UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask Dispose()
        {
            return UniTask.CompletedTask;
        }
    }
}
