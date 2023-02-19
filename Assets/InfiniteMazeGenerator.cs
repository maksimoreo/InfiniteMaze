using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : BaseMazeGenerator
{
    [Range(0f, 1f)]
    [Tooltip("Higher value - more branches, smaller value - less branches")]
    public float branchFactor = 0.2f;

    public NodeGenerator nodeGenerator;

    public override Node GenerateRoot()
    {
        Node root = nodeGenerator.Generate();
        GenerateNeighbours(root);
        return root;
    }

    public override void GenerateNeighbours(Node node)
    {
        if (node.generated) return;

        // Create list of neighbours. It is expected to contain exactly 3 neighbours.
        List<Direction> neighbourDirections = new List<Direction>();
        for (int i = 0; i < 4; i++)
        {
            if (node.connections[i] == null)
                neighbourDirections.Add((Direction)i);
        }

        Utils.Shuffle(neighbourDirections);

        // Create first pass, so that we don't have dead ends
        GenerateAndConnect(node, neighbourDirections[0]);

        // Decide to create other passes
        for (int i = 1; i < neighbourDirections.Count; i++)
        {
            if (Random.value < branchFactor)
                GenerateAndConnect(node, neighbourDirections[i]);
        }

        // Done :)
        node.generated = true;
    }

    private Node GenerateAndConnect(Node node, Direction direction)
    {
        Node newNode = nodeGenerator.Generate();
        node.Connect(direction, newNode);
        return newNode;
    }
}
