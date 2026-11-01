using System;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class DebugRobotAttack : MonoBehaviour
    {
        [SerializeField] private float time;
        [SerializeField] private Vector2Int boardSize;
        [SerializeField] private RobotAttackConfig config;

        private float cubeSize = 5f;
         
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            for (int x = 0; x < boardSize.x; x++)
            {
                for (int y = 0; y < boardSize.y; y++)
                {
                    Vector3 position = transform.position + new Vector3(x * cubeSize, y * cubeSize, 0f);
                    Gizmos.DrawWireCube(position, Vector3.one * cubeSize);
                }
            }
            
            foreach (var attack in config.attackList)
            {
                if (attack.time > time || Mathf.Abs(time - attack.time) > 1f) continue;

                if (attack.attackType == RobotItemType.OneBlock || attack.attackType == RobotItemType.BeatableBlock)
                {
                    Gizmos.color = attack.attackType == RobotItemType.OneBlock ? Color.red : Color.yellow;
                    Vector3 position = (Vector2)transform.position + (Vector2)attack.position * cubeSize;
                    Gizmos.DrawWireSphere(position, 2f);
                }
                else if (attack.attackType == RobotItemType.TwoBlock)
                {
                    Gizmos.color = Color.red;
                    for (int i = 0; i < 2; i++)
                    {
                        Vector3 position = (Vector2)transform.position + (attack.position + (Vector2.right * i)) * cubeSize;
                        Gizmos.DrawWireSphere(position, 2f);
                    }
                }
                else if (attack.attackType == RobotItemType.FourBlock)
                {
                    Gizmos.color = Color.red;
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            Vector3 position = (Vector2)transform.position + (attack.position + (Vector2.right * x) + (Vector2.up * y)) * cubeSize;
                            Gizmos.DrawWireSphere(position, 2f);
                        }
                    }
                }
                else if (attack.attackType == RobotItemType.TargetBlock)
                {
                    Gizmos.color = Color.red;
                    Vector3 position = (Vector2)transform.position + (Vector2)(boardSize - Vector2.one) * 0.5f * cubeSize;
                    Gizmos.DrawWireCube(position, (Vector2)boardSize * cubeSize * 1.05f);
                }
            }
        }
    }
}
