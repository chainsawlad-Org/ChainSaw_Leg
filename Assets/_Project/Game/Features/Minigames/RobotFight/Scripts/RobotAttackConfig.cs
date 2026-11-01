using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    [CreateAssetMenu(fileName = "RobotAttack", menuName = "MyGame/Robot Attack")]
    public class RobotAttackConfig : ScriptableObject
    {
        public RobotAttack[] attackList;
    }

    [Serializable]
    public struct RobotAttack
    {
        public float time;
        public RobotItemType attackType;
        public Vector2Int position;
    }
    
    public enum RobotItemType
    {
        OneBlock,
        TwoBlock,
        FourBlock,
        BeatableBlock,
        TargetBlock
    }
}
