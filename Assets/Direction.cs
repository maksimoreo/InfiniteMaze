using UnityEngine;

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

static class DirectionUtils
{
    public static string ToString(this Direction d)
    {
        switch (d)
        {
            case Direction.Up: return "Up";
            case Direction.Right: return "Right";
            case Direction.Down: return "Down";
            case Direction.Left: return "Left";
            default: return "Undefined [" + (int)d + "]";
        }
    }

    public static Direction ToRight(this Direction d)
    {
        return d.Add(1);
    }

    public static Direction ToLeft(this Direction d)
    {
        return d.Add(-1);
    }

    public static Direction ToOpposite(this Direction d)
    {
        return d.Add(2);
    }

    public static Direction Add(this Direction d, int n)
    {
        // https://stackoverflow.com/a/1082938
        int newD = (int)(d + n) % 4;
        return (Direction)(newD < 0 ? newD + 4 : newD);
        // return (Direction)((int)(d + n) % 4);
    }

    public static Vector2Int ToVector2Int(this Direction d)
    {
        switch (d)
        {
            case Direction.Up:
                return Vector2Int.up;
            case Direction.Right:
                return Vector2Int.right;
            case Direction.Down:
                return Vector2Int.down;
            default:
                return Vector2Int.left;
        }
    }

    public static Direction FromVector2Int(Vector2Int v)
    {
        if (v.x == 0 && v.y == 1)
            return Direction.Up;
        else if (v.x == 1 && v.y == 0)
            return Direction.Right;
        else if (v.x == 0 && v.y == -1)
            return Direction.Down;
        else if (v.x == -1 && v.y == 0)
            return Direction.Left;

        throw new System.ArgumentException("Cannot convert " + v.ToString() + " to Direction");
    }

    public static Vector2Int AdvanceVector(Vector2Int v, Direction d)
    {
        return v + d.ToVector2Int();
    }
}
