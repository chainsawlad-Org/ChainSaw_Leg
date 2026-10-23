using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChainSawLeg.Features.Minigames
{
    public class AttackManager : MonoBehaviour
    {
        [SerializeField] private ArenaBoard arenaBoard;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private GameObject twoBlocksPrefab;
        [SerializeField] private GameObject fourBlockPrefab;
        [SerializeField] private GameObject beatableBlockPrefab;
        [SerializeField] private Transform itemSpawnPoint;
        [SerializeField] private RobotAttackConfig attackConfig;
        [SerializeField] private RobotAttackConfig secondAttackConfig;

        private float itemFlyDuration = 1f;
        private bool itemSpawnInHeaven;

        private void Start()
        {
            StartSequence();
        }

        private void StartSequence()
        {
            Sequence sequence = DOTween.Sequence();
            foreach (var attack in attackConfig.attackList)
            {
                //sequence.Join(DOVirtual.DelayedCall(attack.time, () => Attack(attack.position, attack.attackType))).SetLoops(-1);
                sequence.Join(DOVirtual.DelayedCall(attack.time, () => Attack(attack.position, attack.attackType)));
            }

            sequence.Append(DOVirtual.DelayedCall(1f, () =>
            {
                arenaBoard.TestExpandArena();
                itemSpawnInHeaven = true;
            }));

            foreach (var attack in secondAttackConfig.attackList)
            {
                sequence.Join(DOVirtual.DelayedCall(attack.time, () => Attack(attack.position, attack.attackType)));
            }
        }

        private void Attack(Vector2Int cell, RobotItemType attackType)
        {
            switch (attackType)
            {
                case RobotItemType.OneBlock: AttackOneCell(cell); break;
                case RobotItemType.TargetBlock: AttackOneCell(arenaBoard.currentPlayerCell); break;
                case RobotItemType.TwoBlock: AttackTwoCells(cell); break;
                case RobotItemType.FourBlock: AttackFourCell(cell); break;
                case RobotItemType.BeatableBlock: TryAttackCell(cell); break;
            }
        }

        private void AttackCells(Vector2 targetPos, Vector2Int[] cells, GameObject prefab)
        {
            GameObject item = SpawnItem(prefab, targetPos, itemSpawnInHeaven);

            foreach (var cell in cells)
            {
                arenaBoard.SetCellDanger(cell);
            
                DOVirtual.DelayedCall(itemFlyDuration, () =>
                {
                    arenaBoard.TakeDamageToCell(cell, 10);
                    arenaBoard.SetCellDefault(cell);
                    Destroy(item.gameObject);
                });
            }
        }

        private void AttackOneCell(Vector2Int targetCell)
        {
            if (!arenaBoard.CellIsAvailable(targetCell)) return;
            AttackCells(arenaBoard.GetCellPosition(targetCell), new Vector2Int[] {targetCell}, itemPrefab);
        }

        private void AttackTwoCells(Vector2Int targetCell)
        {
            Vector2Int[] cells = new Vector2Int[] { targetCell, targetCell + Vector2Int.right };
            foreach (var cell in cells) if (!arenaBoard.CellIsAvailable(cell)) return;
            Vector2 position = (arenaBoard.GetCellPosition(cells[0]) + arenaBoard.GetCellPosition(cells[1])) / 2f;
            AttackCells(position, cells, twoBlocksPrefab);
        }

        private void AttackFourCell(Vector2Int targetCell)
        {
            Vector2Int[] cells = new Vector2Int[] { targetCell, targetCell + Vector2Int.right, targetCell + Vector2Int.up, targetCell + Vector2Int.one };
            foreach (var cell in cells) if (!arenaBoard.CellIsAvailable(cell)) return;
            Vector2 position = (arenaBoard.GetCellPosition(cells[0]) + arenaBoard.GetCellPosition(cells[3])) / 2f;
            AttackCells(position, cells, fourBlockPrefab);
        }

        private void TryAttackCell(Vector2Int targetCell)
        {
            GameObject item = SpawnItem(beatableBlockPrefab, arenaBoard.GetCellPosition(targetCell), itemSpawnInHeaven);

            arenaBoard.SetCellBeatable(targetCell);
            
            DOVirtual.DelayedCall(itemFlyDuration, () =>
            {
                if (arenaBoard.CellIsBeated(targetCell))
                {
                    item.transform.DOKill();
                    item.transform.DOMove(itemSpawnPoint.position, 0.75f).SetEase(Ease.Linear)
                        .OnComplete(() => Destroy(item.gameObject));
                }
                else
                {
                    arenaBoard.TakeDamageToCell(targetCell, 10);
                    Destroy(item.gameObject);
                }
                arenaBoard.SetCellDefault(targetCell);
            });
        }

        private GameObject SpawnItem(GameObject prefab, Vector2 targetPosition, bool fromHeaven)
        {
            Vector3 spawnPoint = fromHeaven ? targetPosition + Vector2.up * 60f : itemSpawnPoint.position;
            GameObject item = Instantiate(prefab, spawnPoint, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

            if (fromHeaven)
            {
                item.transform.DOMove(targetPosition, 1f)
                    .SetEase(Ease.Linear)
                    .OnUpdate(() => item.transform.Rotate(Vector3.forward, Time.deltaTime * 100f));
            }
            else
            {
                item.transform.DOJump(targetPosition, 15f, 1, itemFlyDuration)
                    .SetEase(Ease.Linear)
                    .OnUpdate(() => item.transform.Rotate(Vector3.forward, Time.deltaTime * 100f));
            }
            
            return item;
        }
    }
}
