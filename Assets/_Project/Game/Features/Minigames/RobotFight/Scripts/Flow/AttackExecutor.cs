using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Executes single robot attacks: resolves affected cells per attack type, checks
    /// availability, launches the matching item and applies impact effects (damage or
    /// beatable kick-back). Replaces the duplicated per-type attack code.
    /// </summary>
    public class AttackExecutor
    {
        private readonly ArenaBoard arenaBoard;
        private readonly RobotKiller robot;
        private readonly RobotFightConfig config;
        private readonly ItemLauncher launcher;
        private readonly IRobotFightAudio audio;
        private readonly GameObject singleItemPrefab;
        private readonly GameObject doubleItemPrefab;
        private readonly GameObject quadItemPrefab;
        private readonly GameObject beatableItemPrefab;
        private readonly Action onBeatableKicked;
        private readonly Action onBeatReturned;

        /// <summary>
        /// Creates the executor.
        /// </summary>
        /// <param name="onBeatableKicked">Raised the moment a beatable block is kicked back (stops the running wave).</param>
        /// <param name="onBeatReturned">Raised after the kicked block returns and the robot takes the hit (advances the fight).</param>
        public AttackExecutor(ArenaBoard arenaBoard, RobotKiller robot, RobotFightConfig config,
            ItemLauncher launcher, IRobotFightAudio audio,
            GameObject singleItemPrefab, GameObject doubleItemPrefab, GameObject quadItemPrefab,
            GameObject beatableItemPrefab, Action onBeatableKicked, Action onBeatReturned)
        {
            this.arenaBoard = arenaBoard;
            this.robot = robot;
            this.config = config;
            this.launcher = launcher;
            this.audio = audio;
            this.singleItemPrefab = singleItemPrefab;
            this.doubleItemPrefab = doubleItemPrefab;
            this.quadItemPrefab = quadItemPrefab;
            this.beatableItemPrefab = beatableItemPrefab;
            this.onBeatableKicked = onBeatableKicked;
            this.onBeatReturned = onBeatReturned;
        }

        /// <summary>Executes one attack from a wave config.</summary>
        /// <param name="fromHeaven">True spawns the item above the target instead of throwing from the robot.</param>
        public void Execute(RobotAttack attack, bool fromHeaven)
        {
            switch (attack.attackType)
            {
                case RobotItemType.OneBlock:
                    AttackOneCell(attack.position, fromHeaven);
                    break;
                case RobotItemType.TargetBlock:
                    AttackOneCell(arenaBoard.currentPlayerCell, fromHeaven);
                    break;
                case RobotItemType.TwoBlock:
                    AttackTwoCells(attack.position, fromHeaven);
                    break;
                case RobotItemType.FourBlock:
                    AttackFourCells(attack.position, fromHeaven);
                    break;
                case RobotItemType.BeatableBlock:
                    LaunchBeatableBlock(attack.position, fromHeaven);
                    break;
            }
        }

        private void AttackOneCell(Vector2Int targetCell, bool fromHeaven)
        {
            if (!arenaBoard.CellIsAvailable(targetCell)) return;

            AttackCells(arenaBoard.GetCellPosition(targetCell), new[] { targetCell }, singleItemPrefab, fromHeaven);
        }

        private void AttackTwoCells(Vector2Int targetCell, bool fromHeaven)
        {
            Vector2Int[] affectedCells = { targetCell, targetCell + Vector2Int.right };
            foreach (Vector2Int cell in affectedCells)
            {
                if (!arenaBoard.CellIsAvailable(cell)) return;
            }

            Vector2 position = (arenaBoard.GetCellPosition(affectedCells[0]) + arenaBoard.GetCellPosition(affectedCells[1])) / 2f;
            AttackCells(position, affectedCells, doubleItemPrefab, fromHeaven);
        }

        private void AttackFourCells(Vector2Int targetCell, bool fromHeaven)
        {
            Vector2Int[] affectedCells =
                { targetCell, targetCell + Vector2Int.right, targetCell + Vector2Int.up, targetCell + Vector2Int.one };
            foreach (Vector2Int cell in affectedCells)
            {
                if (!arenaBoard.CellIsAvailable(cell)) return;
            }

            Vector2 position = (arenaBoard.GetCellPosition(affectedCells[0]) + arenaBoard.GetCellPosition(affectedCells[3])) / 2f;
            AttackCells(position, affectedCells, quadItemPrefab, fromHeaven);
        }

        private void AttackCells(Vector2 targetPosition, Vector2Int[] affectedCells, GameObject prefab, bool fromHeaven)
        {
            if (!fromHeaven)
            {
                robot.ThrowAnimation();
            }

            GameObject item = launcher.Launch(prefab, targetPosition, fromHeaven);

            foreach (Vector2Int cell in affectedCells)
            {
                arenaBoard.SetCellDanger(cell);

                DOVirtual.DelayedCall(config.ItemFlyDuration, () =>
                {
                    arenaBoard.TakeDamageToCell(cell, config.DamagePerHit);
                    arenaBoard.SetCellDefault(cell);
                    BreakItem(item);
                });
            }
        }

        private void LaunchBeatableBlock(Vector2Int targetCell, bool fromHeaven)
        {
            GameObject item = launcher.Launch(beatableItemPrefab, arenaBoard.GetCellPosition(targetCell), fromHeaven);
            arenaBoard.SetCellBeatable(targetCell);

            DOVirtual.DelayedCall(config.ItemFlyDuration, () =>
            {
                if (arenaBoard.CellIsBeated(targetCell))
                {
                    onBeatableKicked?.Invoke();
                    arenaBoard.FlipPlayerToRobot();
                    item.transform.DOKill();
                    item.transform.DOMove(launcher.SpawnPoint, config.ItemReturnDuration).SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            UnityEngine.Object.Destroy(item.gameObject);
                            robot.OnDamageAnimation(onBeatReturned);
                        });
                    audio.Play(AudioId.AttackHit);
                }
                else
                {
                    arenaBoard.TakeDamageToCell(targetCell, config.DamagePerHit);
                    BreakItem(item);
                }

                arenaBoard.SetCellDefault(targetCell);
            });
        }

        /// <summary>Shared break presentation: hide the item sprite, play its impact particles and break sound.</summary>
        private void BreakItem(GameObject item)
        {
            item.GetComponent<SpriteRenderer>().enabled = false;
            item.GetComponent<ParticleSystem>().Play();
            audio.Play(AudioId.ItemBreak, config.ItemBreakVolume, Random.Range(config.ItemBreakPitchRange.x, config.ItemBreakPitchRange.y));
        }
    }
}
