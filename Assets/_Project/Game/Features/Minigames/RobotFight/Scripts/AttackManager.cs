using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private PlayerMovement player;
        [SerializeField] private RobotKiller robot;
        [SerializeField] private SpriteRenderer blackBackground;
        [SerializeField] private Image whiteScreen;
        [SerializeField] private GameObject[] peopleObjects;
        [SerializeField] private GameObject[] happyPeopleObjects;
        [SerializeField] private RobotAttackConfig[] attackConfigs;

        private float itemFlyDuration = 1f;
        private bool itemSpawnInHeaven;
        private int currentAttackConfig;
        private Sequence attackSequence;
        private Coroutine attackCoroutine;

        private void Start()
        {
            arenaBoard.onLastCellKicked += OnRobotKicked;
            arenaBoard.onPlayerDead += OnPlayerDead;
        }

        public void ReadyUpPlayer(float stunDuration)
        {
            player.StunPlayer();

            DOVirtual.DelayedCall(stunDuration, () =>
            {
                DOVirtual.DelayedCall(4f, () => player.UnstunPlayer());
                player.transform.DOMove(arenaBoard.GetCellPosition(arenaBoard.currentPlayerCell), 1f).SetEase(Ease.Linear);
                arenaBoard.SetupCamera();
            });
        }
        
        public void StartSequence()
        {
            player.gameObject.SetActive(false);
            arenaBoard.EnablePlayer();
            attackSequence = DOTween.Sequence();
            
            foreach (var attack in attackConfigs[currentAttackConfig].attackList)
            {
                attackSequence.Join(DOVirtual.DelayedCall(attack.time, () => Attack(attack.position, attack.attackType)));
            }

            attackSequence.Append(DOVirtual.DelayedCall(2f, StartNextAttack));
        }

        IEnumerator LoopedAttack()
        {
            while (true)
            {
                for (int i = 0; i < attackConfigs[currentAttackConfig].attackList.Length; i++)
                {
                    RobotAttack attack = attackConfigs[currentAttackConfig].attackList[i];
                    float time = attack.time;
                    if (i > 0) time -= attackConfigs[currentAttackConfig].attackList[i - 1].time;
                    yield return new WaitForSeconds(time);

                    Attack(attack.position, attack.attackType);
                }
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
            if (!itemSpawnInHeaven) robot.ThrowAnimation();
            
            GameObject item = SpawnItem(prefab, targetPos, itemSpawnInHeaven);

            foreach (var cell in cells)
            {
                arenaBoard.SetCellDanger(cell);
            
                DOVirtual.DelayedCall(itemFlyDuration, () =>
                {
                    arenaBoard.TakeDamageToCell(cell, 20);
                    arenaBoard.SetCellDefault(cell);
                    item.GetComponent<SpriteRenderer>().enabled = false;
                    item.GetComponent<ParticleSystem>().Play();
                    AudioService.Play(AudioId.ItemBreak, 0.03f, Random.Range(0.85f, 1.1f));
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
                    if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                    arenaBoard.FlipPlayerToRobot();
                    item.transform.DOKill();
                    item.transform.DOMove(itemSpawnPoint.position, 0.2f).SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            Destroy(item.gameObject);
                            robot.OnDamageAnimation(StartNextAttack);
                        });
                    AudioService.Play(AudioId.AttackHit);
                }
                else
                {
                    arenaBoard.TakeDamageToCell(targetCell, 20);
                    item.GetComponent<SpriteRenderer>().enabled = false;
                    item.GetComponent<ParticleSystem>().Play();
                    AudioService.Play(AudioId.ItemBreak, 0.03f, Random.Range(0.85f, 1.1f));
                }
                arenaBoard.SetCellDefault(targetCell);
            });
        }

        private void StartNextAttack()
        {
            if (currentAttackConfig == 3)
            {
                StartSecondPhase();
                return;
            }
            
            currentAttackConfig++;
            
            Debug.Log($"STARTING NEXT ATTACK: {currentAttackConfig}");

            DOVirtual.DelayedCall(3f, () =>
            {
                attackCoroutine = StartCoroutine(LoopedAttack());
            });
        }

        private void StartSecondPhase()
        {
            arenaBoard.isSecondPhase = true;
            itemSpawnInHeaven = true;
            arenaBoard.SetActivePlayer(false);
            arenaBoard.FlipPlayerToRobot();
            Vector2 playerPos = arenaBoard.GetCellPosition(arenaBoard.currentPlayerCell);
            robot.PlayToSecondPhaseAnimation(playerPos, () =>
            {
                blackBackground.gameObject.SetActive(true);
                foreach(var man in peopleObjects) man.SetActive(false);
                DOVirtual.Color(blackBackground.color, Color.black, 1f,
                    onVirtualUpdate: x => blackBackground.color = x);
            }, () =>
            {
                arenaBoard.KickPlayer(Vector2.left * 60f, 0.75f);
                arenaBoard.cameraFlowToPlayer = true;
                currentAttackConfig = 4;
                DOVirtual.DelayedCall(3f, () => arenaBoard.CreateArenaSecondPhase(new Vector2Int(10, 3), () =>
                {
                    robot.transform.position = arenaBoard.GetCellPosition(new Vector2Int(9, 1)) + Vector2.right * 5f;
                    arenaBoard.SetActivePlayer(true);
                    attackCoroutine = StartCoroutine(LoopedAttack());
                }));
            });
        }

        private void OnRobotKicked()
        {
            AudioService.Play(AudioId.AttackHit);
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            currentAttackConfig++;
            
            if (currentAttackConfig == 7)
            {
                EndFight();
                return;
            }
            
            arenaBoard.SetActivePlayer(false);
            arenaBoard.SetCellDefault(arenaBoard.currentPlayerCell);
            robot.OnDamageAnimation(() =>
            {
                Vector2Int arenaSize = new Vector2Int(currentAttackConfig == 5 ? 18 : 25, 3);
                robot.Jump(robot.transform.position + Vector3.right * 60f, () =>
                {
                    arenaBoard.CreateArenaSecondPhase(arenaSize, () =>
                    {
                        arenaBoard.SetActivePlayer(true);
                        robot.transform.position = arenaBoard.GetCellPosition(new Vector2Int(arenaSize.x-1, 1)) + Vector2.right * 5f;
                        attackCoroutine = StartCoroutine(LoopedAttack());
                    });
                });
            });
        }

        private void EndFight()
        {
            arenaBoard.SetActivePlayer(false);
            robot.OnDamageAnimation(null);
            whiteScreen.gameObject.SetActive(true);
            DOVirtual.Color(whiteScreen.color, Color.white, 3f, onVirtualUpdate: x => whiteScreen.color = x)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        player.gameObject.SetActive(true);
                        Destroy(robot.gameObject);
                        arenaBoard.CloseArena(player);
                        blackBackground.gameObject.SetActive(false);
                        Color transparent = new Color(1f, 1f, 1f, 0f);
                        DOVirtual.Color(whiteScreen.color, transparent, 1f, onVirtualUpdate: x => whiteScreen.color = x)
                            .SetEase(Ease.Linear);

                        AudioService.Play(AudioId.KidsCheering);

                    
                        foreach (var man in happyPeopleObjects)
                        {
                            man.SetActive(true);
                            man.transform.DOMove(man.transform.position + Vector3.up * 3f, 0.2f)
                                .SetLoops(Random.Range(20, 50), LoopType.Yoyo)
                                .SetDelay(Random.value);
                        }
                    });
                });
        }

        private void OnPlayerDead()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            arenaBoard.SetActivePlayer(false);
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
