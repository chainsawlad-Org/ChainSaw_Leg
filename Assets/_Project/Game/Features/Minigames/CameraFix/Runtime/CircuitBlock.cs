using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public enum CircuitBlockType
    {
        Start,
        End,
        Straight,
        Turn,
        Modifier,
        Empty
    }

    public class CircuitBlock : MonoBehaviour
    {
        [Header("Block settings")]
        [SerializeField] private CircuitBlockType blockType;
        [SerializeField] private int voltage;
        [SerializeField] private bool isFixed;
        [SerializeField] private bool canBeRemoved;

        public CircuitBlockType Type => blockType;
        public int Voltage => voltage;
        public bool IsFixed => isFixed;
        public bool CanBeRemoved => canBeRemoved;

        public bool IsEmpty => blockType == CircuitBlockType.Empty;

        public void SetBlock(CircuitBlockType newType, int newVoltage, bool newIsFixed, bool newCanBeMoved)
        {
            blockType = newType;
            voltage = newVoltage;
            isFixed = newIsFixed;
            canBeRemoved = newCanBeMoved;
        }
    }
}