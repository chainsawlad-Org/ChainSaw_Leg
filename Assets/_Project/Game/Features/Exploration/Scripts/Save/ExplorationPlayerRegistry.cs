using System;
using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationPlayerRegistry :
        IPlayerPositionProvider,
        IPlayerPositionRestorationTarget
    {
        private IPlayerPositionProvider positionProvider;
        private IPlayerPositionRestorationTarget restorationTarget;

        public event Action PositionRestored;

        public bool IsPlayerAvailable =>
            positionProvider != null && positionProvider.IsPlayerAvailable &&
            restorationTarget != null && restorationTarget.IsPlayerAvailable;
        public float PositionX => GetPositionProvider().PositionX;
        public float PositionY => GetPositionProvider().PositionY;
        public int RegistrationVersion { get; private set; }

        public void Register(
            IPlayerPositionProvider newPositionProvider,
            IPlayerPositionRestorationTarget newRestorationTarget)
        {
            positionProvider = newPositionProvider ??
                throw new GameSaveValidationException("Player position provider is required.");
            restorationTarget = newRestorationTarget ??
                throw new GameSaveValidationException("Player restoration target is required.");
            RegistrationVersion++;
        }

        public void Unregister(
            IPlayerPositionProvider registeredPositionProvider,
            IPlayerPositionRestorationTarget registeredRestorationTarget)
        {
            if (!ReferenceEquals(positionProvider, registeredPositionProvider) ||
                !ReferenceEquals(restorationTarget, registeredRestorationTarget))
                return;

            positionProvider = null;
            restorationTarget = null;
        }

        public void RestorePosition(float positionX, float positionY)
        {
            if (!IsPlayerAvailable)
                throw new GameSaveValidationException("Player is not registered for restoration.");

            restorationTarget.RestorePosition(positionX, positionY);
            PositionRestored?.Invoke();
        }

        public async UniTask WaitForRegistrationAfterAsync(
            int registrationVersion,
            CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(
                () => RegistrationVersion > registrationVersion && IsPlayerAvailable,
                cancellationToken: cancellationToken);
        }

        private IPlayerPositionProvider GetPositionProvider()
        {
            if (!IsPlayerAvailable)
                throw new GameSaveValidationException("Player is not registered for saving.");

            return positionProvider;
        }
    }
}
