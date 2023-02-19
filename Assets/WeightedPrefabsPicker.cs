using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeightedPrefab
{
    public GameObject prefab;
    public float weight;
}

// Given an array with prefabs and their weights, can randomly pick a prefab
// (More weight = more chance to pick this prefab)
public class WeightedPrefabsPicker
{
    private float overallWeight;
    private List<WeightedPrefab> weightedPrefabs;

    public WeightedPrefabsPicker(List<WeightedPrefab> weightedPrefabs)
    {
        // TODO: Copy array?
        this.weightedPrefabs = weightedPrefabs;

        foreach (var weightedPrefab in weightedPrefabs)
            overallWeight += weightedPrefab.weight;
    }

    public int Pick()
    {
        float randomNumber = Random.Range(0f, overallWeight);

        float iWeight = 0f;
        for (int i = 0; i < weightedPrefabs.Count; i++)
        {
            iWeight += weightedPrefabs[i].weight;

            if (randomNumber <= iWeight)
                return i;
        }

        // Impossible case
        return 0;
    }

    public GameObject GetPrefab(int weightedPrefabId)
    {
        return weightedPrefabs[weightedPrefabId].prefab;
    }
}
