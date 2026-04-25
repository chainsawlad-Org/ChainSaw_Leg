using UnityEngine;

namespace ChainSawLeg.Features.Minigames.Snake
{
    public enum SnakeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class SnakeDirectionExtensions
    {
        public static Vector2Int ToVector(this SnakeDirection direction)
        {
            return direction switch
            {
                SnakeDirection.Up => Vector2Int.down,
                SnakeDirection.Down => Vector2Int.up,
                SnakeDirection.Left => Vector2Int.left,
                SnakeDirection.Right => Vector2Int.right,
                _ => Vector2Int.up
            };
        }

        public static bool IsOpposite(this SnakeDirection current, SnakeDirection next)
        {
            return (current == SnakeDirection.Up && next == SnakeDirection.Down)
                   || (current == SnakeDirection.Down && next == SnakeDirection.Up)
                   || (current == SnakeDirection.Left && next == SnakeDirection.Right)
                   || (current == SnakeDirection.Right && next == SnakeDirection.Left);
        }
    }
}