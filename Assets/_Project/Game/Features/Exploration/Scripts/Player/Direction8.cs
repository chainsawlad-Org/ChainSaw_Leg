using UnityEngine;

public enum Direction8
{
    Right = 0,
    UpRight = 1,
    Up = 2,
    UpLeft = 3,
    Left = 4,
    DownLeft = 5,
    Down = 6,
    DownRight = 7
}

public static class Direction8Utility
{
    private const float SectorSizeDegrees = 45f;
    private const float HalfSectorSizeDegrees = SectorSizeDegrees / 2f;

    public static Direction8 FromVector(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Direction8.Up;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float normalizedAngle = Mathf.Repeat(angle + HalfSectorSizeDegrees, 360f);
        int directionIndex = Mathf.FloorToInt(normalizedAngle / SectorSizeDegrees) % 8;

        return (Direction8)directionIndex;
    }

    public static Vector2 ToVector(Direction8 direction)
    {
        float angleRadians = (int)direction * SectorSizeDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
    }
}
