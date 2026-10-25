using System.Collections.Generic;
using System;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Minigames
{
    public class ArenaBoard : MonoBehaviour
    {
        [SerializeField] private RobotFightController controller;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private Transform cameraPoint;
        [SerializeField] private Transform arenaCenterPoint;
        [SerializeField] private RectTransform healthbarTransform;
        [SerializeField] private Image healthbarImage;
        [SerializeField] private CanvasGroup loseScreen;
        [SerializeField] private ArenaCell cellPrefab;
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2Int boardSize;
        [SerializeField] private float cameraPointSpeed = 10f;
        [SerializeField] private int playerHealth = 100;
        
        private enum MoveDirection { Up, Down, Left, Right }
        
        private Dictionary<Vector2Int, ArenaCell> cells = new Dictionary<Vector2Int, ArenaCell>();
        public Vector2Int currentPlayerCell { get; private set; } = Vector2Int.one;
        public bool cameraFlowToPlayer;
        public bool isSecondPhase;
        public Action onLastCellKicked;
        public Action onPlayerDead;
        private int baseHealth;

        private void Start()
        {
            baseHealth = playerHealth;
            for (int x = 0; x < boardSize.x; x++)
            {
                for (int y = 0; y < boardSize.y; y++)
                {
                    Vector2 cellPosition = transform.position + new Vector3(x * cellSize.x, y * cellSize.y, 0);
                    ArenaCell cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                    cells.Add(new Vector2Int(x, y), cell);
                }
            }
        }

        public void EnablePlayer()
        {
            foreach (var cell in cells.Values)
            {
                cell.DoSpawnAnimation();
            }
            controller.gameObject.SetActive(true);
            healthbarTransform.DOMoveX(100f, 1f);
        }

        public void SetupCamera()
        {
            cameraFlow.clampPosition = false;
            cameraPoint.position = cameraFlow.transform.position;
            cameraFlow.player = cameraPoint;
            cameraPoint.DOMove(arenaCenterPoint.position, 1f);
        }

        public void CloseArena(PlayerMovement player)
        {
            cameraFlow.clampPosition = true;
            cameraFlow.transform.position = player.transform.position;
            cameraFlow.player = player.transform;
            foreach (var cell in cells.Values) Destroy(cell.gameObject);
            cells.Clear();
        }

        public void FlipPlayerToRobot()
        {
            controller.FlipPlayer(false);
        }

        public void KickPlayer(Vector2 force, float duration)
        {
            controller.Relocate((Vector2)controller.transform.position + force, duration);
            controller.ApplyDamage(0);
        }

        public void CreateArenaSecondPhase(Vector2Int size, Action onComplete)
        {
            currentPlayerCell = Vector2Int.up;

            foreach (var cell in cells.Values) cell.Despawn();
            cells.Clear();

            DOVirtual.DelayedCall(1f, () =>
            {
                boardSize = size;
                
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Vector2 cellPosition = transform.position + new Vector3(x * cellSize.x, y * cellSize.y, 0);
                        ArenaCell cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform);
                        cell.DoSpawnAnimation();
                        cells.Add(new Vector2Int(x, y), cell);
                    }
                }
                
                transform.position = (Vector2)controller.transform.position - currentPlayerCell * cellSize;
                DOVirtual.DelayedCall(1f, () => onComplete?.Invoke());
                
                cells[new Vector2Int(size.x-1, 1)].SetBeatableCell();
            });
        }

        public bool TryMove(Vector2 moveInput, out Vector2 newPosition)
        {
            newPosition = Vector2.zero;
            
            MoveDirection direction = GetMoveDirection(moveInput);
            
            if (TryGetNextCell(direction, out Vector2Int nextCell))
            {
                newPosition = cells[nextCell].transform.position;
                return true;
            }
            return false;
        }

        public void KickCell()
        {
            if (!isSecondPhase) return;

            if (currentPlayerCell == new Vector2Int(boardSize.x - 1, 1))
            {
                onLastCellKicked?.Invoke();
            }
        }
        
        public void SetCellDefault(Vector2Int cell)
        {
            cells[cell].SetDefaultCell();
        }

        public void SetCellDanger(Vector2Int cell)
        {
            cells[cell].SetDangerCell();
        }

        public void SetCellBeatable(Vector2Int cell)
        {
            cells[cell].SetBeatableCell();
        }

        public void TakeDamageToCell(Vector2Int cell, int damage)
        {
            if (currentPlayerCell == cell)
            {
                if (playerHealth <= 0) return;
                
                playerHealth -= damage;
                DOVirtual.Float(healthbarImage.fillAmount, (float)playerHealth / (float)baseHealth, 0.25f, onVirtualUpdate: x => healthbarImage.fillAmount = x);
                controller.ApplyDamage(damage);
                if (playerHealth <= 0)
                {
                    DOVirtual.Float(0f, 1f, 1.5f, onVirtualUpdate: x => loseScreen.alpha = x);
                    healthbarTransform.DOShakePosition(10f, 2f, 100, 90F, false, false);
                    onPlayerDead?.Invoke();
                }
            }
            else cameraFlow.ShakeEffect(0.2f, 100, 0.4f);
        }
        
        public Vector2 GetCellPosition(Vector2Int cell) => cells[cell].transform.position;
        
        public Vector2Int GetBoardSize() => boardSize;

        public bool CellIsAvailable(Vector2Int cell)
        {
            if (!cells.ContainsKey(cell)) return false;
            return cells[cell].inDefaultState;
        }

        public bool CellIsBeated(Vector2Int cell)
        {
            if (currentPlayerCell == cell && Time.time - controller.lastKickTime < 0.25f) return true;
            return false;
        }

        public void SetActivePlayer(bool active)
        {
            controller.controlActive = active;
        }

        private void Update()
        {
            if (!cameraFlowToPlayer) return;

            Vector2 direction = (controller.transform.position - cameraPoint.position).normalized;
            cameraPoint.Translate( Vector2.right * direction.x * Time.deltaTime * cameraPointSpeed);
        }

        private bool TryGetNextCell(MoveDirection direction, out Vector2Int newCell)
        {
            newCell = Vector2Int.zero;
            
            switch (direction)
            {
                case MoveDirection.Up:
                    if (currentPlayerCell.y == boardSize.y - 1) return false;
                    currentPlayerCell += Vector2Int.up;
                    newCell = currentPlayerCell;
                    break;
                case MoveDirection.Down:
                    if (currentPlayerCell.y == 0) return false;
                    currentPlayerCell += Vector2Int.down;
                    newCell = currentPlayerCell;
                    break;
                case MoveDirection.Left:
                    if (currentPlayerCell.x == 0) return false;
                    currentPlayerCell += Vector2Int.left;
                    newCell = currentPlayerCell;
                    break;
                case MoveDirection.Right:
                    if (currentPlayerCell.x == boardSize.x - 1) return false;
                    currentPlayerCell += Vector2Int.right;
                    newCell = currentPlayerCell;
                    break;
            }

            return true;
        }

        private MoveDirection GetMoveDirection(Vector2 input)
        {
            if (input.x > 0) return MoveDirection.Right;
            else if (input.x < 0) return MoveDirection.Left;
            else if (input.y > 0) return MoveDirection.Up;
            else return MoveDirection.Down;
        }
    }
}
