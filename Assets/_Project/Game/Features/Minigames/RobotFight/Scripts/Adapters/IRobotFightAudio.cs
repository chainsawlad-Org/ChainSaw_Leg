namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Audio playback used by the fight. Isolates RobotFight from the static
    /// <c>AudioService</c> so orchestration logic can be tested without a scene.
    /// </summary>
    public interface IRobotFightAudio
    {
        /// <summary>Plays a sound by its generated identifier.</summary>
        /// <param name="id">Sound identifier from the generated AudioId enum.</param>
        /// <param name="volume">Playback volume.</param>
        /// <param name="pitch">Playback pitch.</param>
        void Play(AudioId id, float volume = 1f, float pitch = 1f);
    }
}
