using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// One scripted attack wave of the robot fight: an ordered list of timed attacks.
    /// Asset instances live in <c>Assets/_Project/Content/MiniGames/RobotFight</c> and are
    /// wired into <see cref="AttackManager"/> in SC_World.
    /// </summary>
    [CreateAssetMenu(fileName = "RobotAttack", menuName = "ChainSawLeg/Minigames/RobotFight Attack")]
    public class RobotAttackConfig : ScriptableObject
    {
        /// <summary>Timed attacks of this wave, in chronological order.</summary>
        public RobotAttack[] attackList;
    }
}
