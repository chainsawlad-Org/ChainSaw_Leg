using UnityEngine;

namespace ChainSawLeg.Features.Minigames.Snake
{
    [CreateAssetMenu(
        fileName = "SO_SnakeSessionConfig",
        menuName = "ChainSawLeg/Minigames/Snake Session Config")]
    public sealed class SnakeSessionConfig : ScriptableObject
    {
        [Header("Board")]
        [SerializeField] private int boardWidth = 12;
        [SerializeField] private int boardHeight = 12;
        [SerializeField] private int startingLength = 3;

        [Header("Timing")]
        [SerializeField] private float stepIntervalSeconds = 0.18f;
        [SerializeField] private float sessionDurationSeconds = 45f;

        public int BoardWidth => boardWidth;
        public int BoardHeight => boardHeight;
        public int StartingLength => startingLength;
        public float StepIntervalSeconds => stepIntervalSeconds;
        public float SessionDurationSeconds => sessionDurationSeconds;
    }
}