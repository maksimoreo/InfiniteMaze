using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector2Int Advance(this Vector2Int v, Direction d)
    {
        return DirectionUtils.AdvanceVector(v, d);
    }

    // Note: Modifies given list
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count);

            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public static bool Decide(float probability)
    {
        return Random.value < probability;
    }

    public static T Sample<T>(List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}
