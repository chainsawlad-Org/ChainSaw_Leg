using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Spawns throwable items and animates their flight to the target cell: a jump arc
    /// when thrown by hand from the robot, or a straight rotating drop from above
    /// ("heaven") during the second phase.
    /// </summary>
    public class ItemLauncher
    {
        private readonly RobotFightConfig config;
        private readonly Transform itemSpawnPoint;

        /// <summary>Creates the launcher. <paramref name="itemSpawnPoint"/> is the robot's hand position for hand throws.</summary>
        public ItemLauncher(RobotFightConfig config, Transform itemSpawnPoint)
        {
            this.config = config;
            this.itemSpawnPoint = itemSpawnPoint;
        }

        /// <summary>Spawn position used for hand throws (the robot's hand).</summary>
        public Vector3 SpawnPoint => itemSpawnPoint.position;

        /// <summary>Instantiates <paramref name="prefab"/> and animates it to <paramref name="targetPosition"/>.</summary>
        /// <param name="fromHeaven">True drops the item from above the target; false throws it from the robot's hand.</param>
        public GameObject Launch(GameObject prefab, Vector2 targetPosition, bool fromHeaven)
        {
            Vector3 spawnPosition = fromHeaven
                ? targetPosition + Vector2.up * config.HeavenSpawnOffset
                : itemSpawnPoint.position;
            GameObject item = Object.Instantiate(prefab, spawnPosition, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

            if (fromHeaven)
            {
                item.transform.DOMove(targetPosition, config.ItemFlyDuration)
                    .SetEase(Ease.Linear)
                    .OnUpdate(() => item.transform.Rotate(Vector3.forward, Time.deltaTime * config.ItemSpinSpeed));
            }
            else
            {
                item.transform.DOJump(targetPosition, config.ItemThrowJumpHeight, 1, config.ItemFlyDuration)
                    .SetEase(Ease.Linear)
                    .OnUpdate(() => item.transform.Rotate(Vector3.forward, Time.deltaTime * config.ItemSpinSpeed));
            }

            return item;
        }
    }
}
