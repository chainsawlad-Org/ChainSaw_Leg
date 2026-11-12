using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Pure model of the arena grid: board size, the player's logical cell, and
    /// cell-index-to-world-position math. Knows nothing about views or engine timing,
    /// so it can be unit-tested in isolation.
    /// </summary>
    public class ArenaGridModel
    {
        private readonly Vector2 cellSize;
        private Vector2Int boardSize;

        /// <summary>Creates a grid model; <paramref name="playerCell"/> is the cell the player occupies at start.</summary>
        public ArenaGridModel(Vector2Int boardSize, Vector2 cellSize, Vector2Int playerCell)
        {
            this.boardSize = boardSize;
            this.cellSize = cellSize;
            PlayerCell = playerCell;
        }

        /// <summary>Current board dimensions in cells.</summary>
        public Vector2Int BoardSize => boardSize;

        /// <summary>Logical cell the player currently occupies.</summary>
        public Vector2Int PlayerCell { get; private set; }

        /// <summary>
        /// Second cell of the last column — the beatable-block cell of rebuilt arenas
        /// and the kick target that ends the second phase.
        /// </summary>
        public Vector2Int BeatableCell => new Vector2Int(boardSize.x - 1, 1);

        /// <summary>Replaces the board size when the arena is rebuilt for a new phase.</summary>
        public void Resize(Vector2Int newSize) => boardSize = newSize;

        /// <summary>Teleports the player's logical cell (used when the arena is rebuilt).</summary>
        public void SetPlayerCell(Vector2Int cell) => PlayerCell = cell;

        /// <summary>World position of a cell relative to the board's origin.</summary>
        /// <param name="boardOrigin">World position of the board's origin transform.</param>
        /// <param name="cell">Cell index.</param>
        public Vector2 GetCellWorldPosition(Vector2 boardOrigin, Vector2Int cell) =>
            boardOrigin + new Vector2(cell.x * cellSize.x, cell.y * cellSize.y);

        /// <summary>
        /// Attempts to move the player one cell in the dominant input direction
        /// (horizontal input wins over vertical, matching the original controller).
        /// On success the player's cell is updated and <paramref name="nextCell"/> holds the target.
        /// </summary>
        public bool TryMove(Vector2 input, out Vector2Int nextCell)
        {
            nextCell = PlayerCell;

            Vector2Int target = PlayerCell + ResolveDirection(input);
            if (!IsInside(target)) return false;

            PlayerCell = target;
            nextCell = target;
            return true;
        }

        private Vector2Int ResolveDirection(Vector2 input)
        {
            if (input.x > 0f) return Vector2Int.right;
            if (input.x < 0f) return Vector2Int.left;
            if (input.y > 0f) return Vector2Int.up;
            return Vector2Int.down;
        }

        private bool IsInside(Vector2Int cell) =>
            cell.x >= 0 && cell.x < boardSize.x && cell.y >= 0 && cell.y < boardSize.y;
    }
}
