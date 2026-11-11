namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// What the robot needs from the fight orchestration when it gets kicked into the
    /// arena. Implemented by <see cref="AttackManager"/>; consumed by <see cref="RobotKiller"/>
    /// so the robot does not depend on the orchestrator's concrete type.
    /// </summary>
    public interface IRobotFightStarter
    {
        /// <summary>Stuns the world player and prepares the arena for their arrival.</summary>
        /// <param name="stunDuration">Seconds the world player stays stunned before being pulled in.</param>
        void ReadyUpPlayer(float stunDuration);

        /// <summary>Starts the scripted intro wave once the robot has landed on the roof.</summary>
        void StartSequence();
    }
}
