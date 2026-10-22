using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChainSawLeg.Features.Minigames
{
    public class AttackManager : MonoBehaviour
    {
        [SerializeField] private ArenaBoard arenaBoard;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Transform itemSpawnPoint;
        private float attackRate = 1f;
        private float lastAttackTime;

        private void Update()
        {
            attackRate -= Time.deltaTime / 20f;
            if (Time.time < lastAttackTime + attackRate) return;
            
            lastAttackTime = Time.time;

            RandomAttack();
        }

        private void RandomAttack()
        {
            if (Random.value > 0.5f)
            {
                int multiplier = Random.value > 0.75f ? 2 : 1;
                List<Vector2Int> cells = new List<Vector2Int>();
                
                Vector2Int cellUp = arenaBoard.currentPlayerCell + Vector2Int.up * multiplier;
                if (arenaBoard.CellIsAvailable(cellUp)) cells.Add(cellUp);
                
                Vector2Int cellDown = arenaBoard.currentPlayerCell + Vector2Int.down * multiplier;
                if (arenaBoard.CellIsAvailable(cellDown)) cells.Add(cellDown);
                
                Vector2Int cellLeft = arenaBoard.currentPlayerCell + Vector2Int.left * multiplier;
                if (arenaBoard.CellIsAvailable(cellLeft)) cells.Add(cellLeft);
                
                Vector2Int cellRight = arenaBoard.currentPlayerCell + Vector2Int.right * multiplier;
                if (arenaBoard.CellIsAvailable(cellRight)) cells.Add(cellRight);
                
                if (cells.Count > 0) AttackCell(cells[Random.Range(0, cells.Count)]);
            }
            else if (arenaBoard.CellIsAvailable(arenaBoard.currentPlayerCell)) AttackCell(arenaBoard.currentPlayerCell);
        }

        private void AttackCell(Vector2Int cell)
        {
            float flyDuration = 1f;
            
            GameObject item = Instantiate(itemPrefab, itemSpawnPoint.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
            item.transform.DOJump(arenaBoard.GetCellPosition(cell), 15f, 1, flyDuration)
                .SetEase(Ease.Linear)
                .OnUpdate(() => item.transform.Rotate(Vector3.forward, Time.deltaTime * 100f))
                .OnComplete(() => Destroy(item.gameObject));
            
            
            arenaBoard.SetCellDanger(cell);
            
            DOVirtual.DelayedCall(flyDuration, () =>
            {
                arenaBoard.TakeDamageToCell(cell, 10);
                arenaBoard.SetCellDefault(cell);
            });
        }
    }
}
