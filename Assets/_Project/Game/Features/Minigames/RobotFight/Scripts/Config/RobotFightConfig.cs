using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Central tuning for the RobotFight minigame. Every default reproduces the original
    /// hardcoded behaviour exactly; adjust from the <c>SO_RobotFightConfig</c> asset in
    /// <c>Assets/_Project/Content/MiniGames/RobotFight</c>.
    /// Covers the fight orchestration values owned by <see cref="AttackManager"/>; purely
    /// local animation values stay as serialized fields on their own components.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_RobotFightConfig", menuName = "ChainSawLeg/Minigames/RobotFight Config")]
    public class RobotFightConfig : ScriptableObject
    {
        [Header("Combat")]
        [Tooltip("Damage dealt to the player when a thrown item lands on their cell.")]
        [SerializeField] private int damagePerHit = 20;

        [Tooltip("Seconds between one attack wave finishing and the next wave starting.")]
        [SerializeField] private float nextWaveDelay = 1f;

        [Tooltip("Seconds added after the scripted intro wave before the looped waves begin.")]
        [SerializeField] private float scriptedWaveEndDelay = 2f;

        [Tooltip("Seconds before the world player is un-stunned after being pulled into the arena.")]
        [SerializeField] private float playerUnstunDelay = 4f;

        [Tooltip("Seconds the player reposition move takes when entering the arena.")]
        [SerializeField] private float playerRepositionDuration = 1f;

        [Header("Items")]
        [Tooltip("Seconds a thrown item needs to reach its target cell.")]
        [SerializeField] private float itemFlyDuration = 1f;

        [Tooltip("Height above a target cell that items dropped from 'heaven' spawn at.")]
        [SerializeField] private float heavenSpawnOffset = 60f;

        [Tooltip("Degrees per second an airborne item spins.")]
        [SerializeField] private float itemSpinSpeed = 100f;

        [Tooltip("Jump arc height of an item thrown by hand from the robot.")]
        [SerializeField] private float itemThrowJumpHeight = 15f;

        [Tooltip("Seconds a kicked-back beatable block needs to fly back to the robot.")]
        [SerializeField] private float itemReturnDuration = 0.2f;

        [Tooltip("Volume of the item-break sound.")]
        [SerializeField] private float itemBreakVolume = 0.03f;

        [Tooltip("Min/max random pitch of the item-break sound.")]
        [SerializeField] private Vector2 itemBreakPitchRange = new Vector2(0.85f, 1.1f);

        [Header("Arena Phases")]
        [Tooltip("Arena size built when the second phase starts.")]
        [SerializeField] private Vector2Int secondPhaseArenaSize = new Vector2Int(10, 3);

        [Tooltip("Arena size rebuilt after the first robot kick of the second phase.")]
        [SerializeField] private Vector2Int wideArenaSize = new Vector2Int(18, 3);

        [Tooltip("Arena size rebuilt after the second robot kick of the second phase.")]
        [SerializeField] private Vector2Int widestArenaSize = new Vector2Int(25, 3);

        [Tooltip("Horizontal offset where the robot lands beside the last arena column.")]
        [SerializeField] private float robotLandingOffset = 5f;

        [Tooltip("Seconds between the second-phase intro kick and the arena rebuild.")]
        [SerializeField] private float arenaRebuildDelay = 3f;

        [Header("Cinematics")]
        [Tooltip("Seconds the black background fade-in takes at the second phase start.")]
        [SerializeField] private float backgroundFadeDuration = 1f;

        [Tooltip("Force applied to the player when kicked out during the second-phase intro.")]
        [SerializeField] private float playerKickForce = 60f;

        [Tooltip("Seconds the player kick-out move takes.")]
        [SerializeField] private float playerKickDuration = 0.75f;

        [Tooltip("Distance the robot leaps sideways between second-phase arenas.")]
        [SerializeField] private float robotLeapDistance = 60f;

        [Tooltip("Seconds the white screen fade-in takes at the fight end.")]
        [SerializeField] private float whiteScreenFadeInDuration = 3f;

        [Tooltip("Seconds the white screen stays opaque before the arena is revealed again.")]
        [SerializeField] private float whiteScreenHoldDuration = 3f;

        [Tooltip("Seconds the white screen fade-out takes after the fight end.")]
        [SerializeField] private float whiteScreenFadeOutDuration = 1f;

        [Header("Celebration")]
        [Tooltip("Jump height of celebrating NPCs at the fight end.")]
        [SerializeField] private float celebrateJumpHeight = 3f;

        [Tooltip("Seconds one celebrating NPC jump takes.")]
        [SerializeField] private float celebrateJumpDuration = 0.2f;

        [Tooltip("Min/max (exclusive upper bound) yoyo loop count of a celebrating NPC.")]
        [SerializeField] private Vector2Int celebrateLoopRange = new Vector2Int(20, 50);

        /// <summary>Damage dealt to the player when a thrown item lands on their cell.</summary>
        public int DamagePerHit => damagePerHit;

        /// <summary>Seconds between one attack wave finishing and the next wave starting.</summary>
        public float NextWaveDelay => nextWaveDelay;

        /// <summary>Seconds added after the scripted intro wave before the looped waves begin.</summary>
        public float ScriptedWaveEndDelay => scriptedWaveEndDelay;

        /// <summary>Seconds before the world player is un-stunned after being pulled into the arena.</summary>
        public float PlayerUnstunDelay => playerUnstunDelay;

        /// <summary>Seconds the player reposition move takes when entering the arena.</summary>
        public float PlayerRepositionDuration => playerRepositionDuration;

        /// <summary>Seconds a thrown item needs to reach its target cell.</summary>
        public float ItemFlyDuration => itemFlyDuration;

        /// <summary>Height above a target cell that items dropped from 'heaven' spawn at.</summary>
        public float HeavenSpawnOffset => heavenSpawnOffset;

        /// <summary>Degrees per second an airborne item spins.</summary>
        public float ItemSpinSpeed => itemSpinSpeed;

        /// <summary>Jump arc height of an item thrown by hand from the robot.</summary>
        public float ItemThrowJumpHeight => itemThrowJumpHeight;

        /// <summary>Seconds a kicked-back beatable block needs to fly back to the robot.</summary>
        public float ItemReturnDuration => itemReturnDuration;

        /// <summary>Volume of the item-break sound.</summary>
        public float ItemBreakVolume => itemBreakVolume;

        /// <summary>Min/max random pitch of the item-break sound.</summary>
        public Vector2 ItemBreakPitchRange => itemBreakPitchRange;

        /// <summary>Arena size built when the second phase starts.</summary>
        public Vector2Int SecondPhaseArenaSize => secondPhaseArenaSize;

        /// <summary>Arena size rebuilt after the first robot kick of the second phase.</summary>
        public Vector2Int WideArenaSize => wideArenaSize;

        /// <summary>Arena size rebuilt after the second robot kick of the second phase.</summary>
        public Vector2Int WidestArenaSize => widestArenaSize;

        /// <summary>Horizontal offset where the robot lands beside the last arena column.</summary>
        public float RobotLandingOffset => robotLandingOffset;

        /// <summary>Seconds between the second-phase intro kick and the arena rebuild.</summary>
        public float ArenaRebuildDelay => arenaRebuildDelay;

        /// <summary>Seconds the black background fade-in takes at the second phase start.</summary>
        public float BackgroundFadeDuration => backgroundFadeDuration;

        /// <summary>Force applied to the player when kicked out during the second-phase intro.</summary>
        public float PlayerKickForce => playerKickForce;

        /// <summary>Seconds the player kick-out move takes.</summary>
        public float PlayerKickDuration => playerKickDuration;

        /// <summary>Distance the robot leaps sideways between second-phase arenas.</summary>
        public float RobotLeapDistance => robotLeapDistance;

        /// <summary>Seconds the white screen fade-in takes at the fight end.</summary>
        public float WhiteScreenFadeInDuration => whiteScreenFadeInDuration;

        /// <summary>Seconds the white screen stays opaque before the arena is revealed again.</summary>
        public float WhiteScreenHoldDuration => whiteScreenHoldDuration;

        /// <summary>Seconds the white screen fade-out takes after the fight end.</summary>
        public float WhiteScreenFadeOutDuration => whiteScreenFadeOutDuration;

        /// <summary>Jump height of celebrating NPCs at the fight end.</summary>
        public float CelebrateJumpHeight => celebrateJumpHeight;

        /// <summary>Seconds one celebrating NPC jump takes.</summary>
        public float CelebrateJumpDuration => celebrateJumpDuration;

        /// <summary>Min/max (exclusive upper bound) yoyo loop count of a celebrating NPC.</summary>
        public Vector2Int CelebrateLoopRange => celebrateLoopRange;
    }
}
