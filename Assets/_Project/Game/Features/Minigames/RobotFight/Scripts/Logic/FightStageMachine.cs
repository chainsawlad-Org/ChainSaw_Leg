namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Pure progression model of the whole fight, replacing the original dual-purpose
    /// magic counter (config array index + phase thresholds 3/5/7). The enum's numeric
    /// order deliberately mirrors the <c>attackConfigs</c> array indices (0-6);
    /// <see cref="Stage.Finale"/> has no config.
    /// </summary>
    public class FightStageMachine
    {
        /// <summary>Fight stages in play order.</summary>
        public enum Stage
        {
            /// <summary>Scripted intro wave (config 0), played once.</summary>
            IntroScripted = 0,
            /// <summary>First looped wave (config 1).</summary>
            Phase1Wave1 = 1,
            /// <summary>Second looped wave (config 2).</summary>
            Phase1Wave2 = 2,
            /// <summary>Final wave of phase 1 (config 3); completing it triggers the second-phase intro.</summary>
            Phase1FinalWave = 3,
            /// <summary>First second-phase arena (config 4; 10x3 board built by the intro).</summary>
            Phase2Arena1 = 4,
            /// <summary>Second second-phase arena (config 5; 18x3 board).</summary>
            Phase2Arena2 = 5,
            /// <summary>Third second-phase arena (config 6; 25x3 board).</summary>
            Phase2Arena3 = 6,
            /// <summary>Fight won.</summary>
            Finale = 7
        }

        /// <summary>Stage the fight is currently in.</summary>
        public Stage Current { get; private set; } = Stage.IntroScripted;

        /// <summary>True once the fight is won.</summary>
        public bool IsFinale => Current == Stage.Finale;

        /// <summary>True while the final wave of phase 1 loops; advancing past it starts the second-phase intro.</summary>
        public bool IsPhase1FinalWave => Current == Stage.Phase1FinalWave;

        /// <summary>Index into the <c>attackConfigs</c> array for the current stage.</summary>
        public int CurrentConfigIndex => (int)Current;

        /// <summary>Moves to the next stage; <see cref="Stage.Finale"/> is terminal.</summary>
        public void Advance()
        {
            if (IsFinale) return;

            Current = (Stage)((int)Current + 1);
        }
    }
}
