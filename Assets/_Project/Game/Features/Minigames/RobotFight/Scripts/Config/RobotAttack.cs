using System;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// One timed attack inside a <see cref="RobotAttackConfig"/> wave: what is thrown, where, and when.
    /// </summary>
    [Serializable]
    public struct RobotAttack
    {
        /// <summary>Delay in seconds from the start of the attack wave.</summary>
        public float time;

        /// <summary>Kind of item thrown and cells covered.</summary>
        public RobotItemType attackType;

        /// <summary>Target grid cell (top-left cell for multi-cell attacks).</summary>
        public Vector2Int position;
    }
}
