using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Editor visualization of a robot attack wave: draws the arena grid and where each
    /// attack inside the configured time window lands. Gizmo-only helper for authoring
    /// attack configs; has no gameplay effect.
    /// </summary>
    public class DebugRobotAttack : MonoBehaviour
    {
        private const float GizmoSphereRadius = 2f;
        private const float TargetTimeWindow = 1f;
        private const float TargetBoxPadding = 1.05f;

        [SerializeField] private float time;
        [SerializeField] private Vector2Int boardSize;
        [SerializeField] private RobotAttackConfig config;
        [Tooltip("World size of one grid cell; must match ArenaBoard.cellSize for the gizmos to align.")]
        [SerializeField] private float cubeSize = 5f;

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

            foreach (RobotAttack attack in config.attackList)
            {
                if (attack.time > time || Mathf.Abs(time - attack.time) > TargetTimeWindow) continue;

                switch (attack.attackType)
                {
                    case RobotItemType.OneBlock:
                        Gizmos.color = Color.red;
                        DrawCellSphere(attack.position);
                        break;
                    case RobotItemType.BeatableBlock:
                        Gizmos.color = Color.yellow;
                        DrawCellSphere(attack.position);
                        break;
                    case RobotItemType.TwoBlock:
                        Gizmos.color = Color.red;
                        for (int i = 0; i < 2; i++)
                        {
                            DrawCellSphere(attack.position + Vector2.right * i);
                        }
                        break;
                    case RobotItemType.FourBlock:
                        Gizmos.color = Color.red;
                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                DrawCellSphere(attack.position + Vector2.right * x + Vector2.up * y);
                            }
                        }
                        break;
                    case RobotItemType.TargetBlock:
                        Gizmos.color = Color.red;
                        Vector3 targetPosition = (Vector2)transform.position + (Vector2)(boardSize - Vector2.one) * 0.5f * cubeSize;
                        Gizmos.DrawWireCube(targetPosition, (Vector2)boardSize * cubeSize * TargetBoxPadding);
                        break;
                }
            }
        }

        private void DrawCellSphere(Vector2 cell)
        {
            Vector3 position = (Vector2)transform.position + cell * cubeSize;
            Gizmos.DrawWireSphere(position, GizmoSphereRadius);
        }
    }
}
