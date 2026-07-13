using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class CheckpointSaveRequestBroker
    {
        public event Func<string, CancellationToken, UniTask<bool>> SaveRequested;

        public UniTask<bool> RequestSaveAsync(string checkpointId, CancellationToken cancellationToken)
        {
            Func<string, CancellationToken, UniTask<bool>> handler = SaveRequested;

            return handler != null
                ? handler(checkpointId, cancellationToken)
                : UniTask.FromResult(false);
        }
    }
}
