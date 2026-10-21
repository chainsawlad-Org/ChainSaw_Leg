using System.Threading;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Application
{
    public interface IBootstrapRunner
    {
        UniTask Run(CancellationToken cancellationToken);
    }
}
