using System.Threading;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Exploration.Save
{
    public interface IExplorationSceneTransitionService
    {
        UniTask ReloadSceneAsync(string sceneId, CancellationToken cancellationToken);
    }
}
