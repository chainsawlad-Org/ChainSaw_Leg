namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Concrete <see cref="IRobotFightAudio"/> implementation backed by the shared static
    /// <c>AudioService</c>.
    /// </summary>
    public class AudioServicePlayer : IRobotFightAudio
    {
        /// <inheritdoc/>
        public void Play(AudioId id, float volume = 1f, float pitch = 1f) =>
            AudioService.Play(id, volume, pitch);
    }
}
