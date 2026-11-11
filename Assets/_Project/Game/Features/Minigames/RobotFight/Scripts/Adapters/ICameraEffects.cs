namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Camera feedback effects used by the fight. Isolates RobotFight from the concrete
    /// <c>CameraFlow</c> API so orchestration logic can be tested without a camera.
    /// </summary>
    public interface ICameraEffects
    {
        /// <summary>Plays a temporary positional shake.</summary>
        /// <param name="strength">Shake strength in world units.</param>
        /// <param name="vibrato">Number of shake oscillations.</param>
        /// <param name="duration">Shake duration in seconds.</param>
        void Shake(float strength, int vibrato, float duration);
    }
}
