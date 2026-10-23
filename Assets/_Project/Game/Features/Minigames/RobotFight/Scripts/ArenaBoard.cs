using System.Collections.Generic;
using System.Linq;
using ChainSawLeg.Features.Exploration;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class ArenaBoard : MonoBehaviour
    {
        [SerializeField] private RobotFightController controller;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private ArenaCell cellPrefab;
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2Int boardSize;
        
        private enum MoveDirection { Up, Down, Left, Right }
        
        private Dictionary<Vector2Int, ArenaCell> cells = new Dictionary<Vector2Int, ArenaCell>();
        public Vector2Int currentPlayerCell { get; private set; }
        
        private void Awake()
        {
            for (int x = 0; x < boardSize.x; x++)
            {
                for (int y = 0; y < boardSize.y; y++)
                {
                    Vector2 cellPosition = transform.position + new Vector3(x * cellSize.x, y * cellSize.y, 0);
                    ArenaCell cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                    cell.DoSpawnAnimation();
                    cells.Add(new Vector2Int(x, y), cell);
                }
            }
        }

        public void TestExpandArena()
        {
            currentPlayerCell = Vector2Int.zero;
            controller.Relocate(GetCellPosition(Vector2Int.zero), 0.5f);
            Vector2Int oldBoardSize = boardSize;
            boardSize = new Vector2Int(9, 3);

            foreach (var cell in cells.ToArray())
            {
                if (cell.Key.y > boardSize.y - 1)
                {
                    cells[cell.Key].DoDespawnAnimation();
                    Destroy(cells[cell.Key].gameObject, 5f);
                    cells.Remove(cell.Key);
                }
            }
            
            for (int x = oldBoardSize.x; x < boardSize.x; x++)
            {
                for (int y = 0; y < boardSize.y; y++)
                {
                    Vector2 cellPosition = transform.position + new Vector3(x * cellSize.x, y * cellSize.y, 0);
                    ArenaCell cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                    cell.DoSpawnAnimation();
                    cells.Add(new Vector2Int(x, y), cell);
                }
            }
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
            if (currentPlayerCell == cell) controller.ApplyDamage(damage);
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
