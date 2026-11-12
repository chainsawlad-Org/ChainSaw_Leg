using ChainSawLeg.Features.Exploration;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Concrete <see cref="ICameraEffects"/> implementation backed by the shared
    /// <see cref="CameraFlow"/> component.
    /// </summary>
    public class CameraFlowEffects : ICameraEffects
    {
        private readonly CameraFlow cameraFlow;

        public CameraFlowEffects(CameraFlow cameraFlow)
        {
            this.cameraFlow = cameraFlow;
        }

        /// <inheritdoc/>
        public void Shake(float strength, int vibrato, float duration) =>
            cameraFlow.ShakeEffect(strength, vibrato, duration);
    }
}
