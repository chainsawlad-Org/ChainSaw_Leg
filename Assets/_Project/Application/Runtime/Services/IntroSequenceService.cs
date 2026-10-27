using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Application
{
    public sealed class IntroSequenceService
    {
        private UniTaskCompletionSource completionSource;

        public void Prepare()
        {
            completionSource = new UniTaskCompletionSource();
        }

        public void Complete()
        {
            completionSource?.TrySetResult();
        }

        public async UniTask WaitForCompletionAsync(
            CancellationToken cancellationToken)
        {
            if (completionSource == null)
            {
                throw new InvalidOperationException(
                    "Intro sequence was not prepared.");
            }

            await completionSource.Task
                .AttachExternalCancellation(cancellationToken);
        }
    }
}