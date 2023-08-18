using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// BFS with depth tracking: https://stackoverflow.com/a/55517515
public class FiniteMazeGenerator : BaseMazeGenerator
{
    public int maxNodeCount;
    public float[] neighbourProbabilities;
    public NodeGenerator nodeGenerator;
    public MazeRenderer mazeRenderer;

    public int additionalConnectionsAttempts = 0;
    public int maxAdditionalConnections = 0;

    [Tooltip("Generator will restructure maze every <this> seconds")]
    public float restructureMazeTimer = 30;

    [Header("Disconnections")]
    public int disconnectAttempts = 0;
    public int maxDisconnects = 0;

    [Header("Connections")]
    public int connectAttempts = 0;
    public int maxConnects = 0;

    [Header("Node randomization")]
    public float randomizeNodesVisualsTimer = 20;
    public int randomizeNodesVisualsAttempts = 0;
    public int maxRandomizeNodesVisuals = 0;

    private List<Node> nodes = new List<Node>();
    private int simpleNodeLimit;
    private Queue<Node> queue = new Queue<Node>();
    private int currentNodeCount = 1; // Starting from root node

    public override Node GenerateRoot()
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Reset state
        queue.Clear();
        nodes.Clear();
        currentNodeCount = 1;
        simpleNodeLimit = maxNodeCount - 1;

        // Step 1: Base structure
        Node root = nodeGenerator.Generate();

        queue.Enqueue(root);

        while (queue.Count != 0 && IsUnderNodeLimit())
        {
            Node node = queue.Dequeue();

            // Generate neighbours
            List<Direction> neighbours = GetEmptyConnections(node);
            Utils.Shuffle(neighbours);

            int startIndex = 0;

            // If no nodes in the queue, always generate pass
            if (queue.Count == 0)
            {
                GenerateAndConnectToNode(neighbours[0], node);
                startIndex = 1;
            }

            for (
                int i = startIndex;

                i < neighbourProbabilities.Length &&
                i < neighbours.Count &&
                IsUnderNodeLimit() &&
                Utils.Decide(neighbourProbabilities[i]);

                i++)
            {
                GenerateAndConnectToNode(neighbours[i], node);
            }
        }

        // Step 2: Exit
        Node last = queue.Dequeue();
        Node other = nodeGenerator.Generate();
        other.isExit = true;
        last.Connect(Utils.Sample(GetEmptyConnections(last)), other);

        // Step 3: Additional connections
        int additionalConnections = 0;
        for (int i = 0; i < additionalConnectionsAttempts && additionalConnections < maxAdditionalConnections; i++)
        {
            Node first = Utils.Sample(nodes);
            Node second = Utils.Sample(nodes);
            Direction direction = (Direction)Random.Range(0, 4);

            if (first.GetConnected(direction) != null || second.GetConnected(direction.ToOpposite()) != null)
                continue;

            first.Connect(direction, second);
            additionalConnections++;
        }

        Debug.Log(
            $"Done GenerateRoot() in {stopwatch.Elapsed}." +
            $" Added {additionalConnections} connections in {additionalConnectionsAttempts} attempts." +
            $" Node queue size: {queue.Count}"
        );

        StartCoroutine(RestructureMaze());
        StartCoroutine(RandomizeNodesVisuals());

        return root;
    }

    public override void GenerateNeighbours(Node node) { }

    private bool IsUnderNodeLimit() => currentNodeCount < simpleNodeLimit;

    private Node GenerateAndConnectToNode(Direction direction, Node node)
    {
        Node newNode = nodeGenerator.Generate();
        node.Connect(direction, newNode);
        queue.Enqueue(newNode);
        nodes.Add(newNode);
        currentNodeCount++;
        return newNode;
    }

    private List<Direction> GetEmptyConnections(Node node)
    {
        List<Direction> neighbourDirections = new List<Direction>();

        for (int i = 0; i < node.connections.Length; i++)
            if (node.connections[i] == null)
                neighbourDirections.Add((Direction)i);

        return neighbourDirections;
    }


    private IEnumerator RestructureMaze()
    {
        while (true)
        {
            yield return new WaitForSeconds(restructureMazeTimer);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Break random connections
            int brokenConnections = 0;
            for (int i = 0; i < disconnectAttempts && brokenConnections < maxDisconnects; i++)
                if (BreakRandomConnection())
                    brokenConnections++;

            // Connect random nodes
            int addedConnections = 0;
            for (int i = 0; i < connectAttempts && addedConnections < maxConnects; i++)
                if (AddRandomConnection())
                    addedConnections++;

            stopwatch.Stop();
            Debug.Log(
                $"Done RestructureMaze() in {stopwatch.Elapsed}." +
                $" Broken {brokenConnections} connections in {disconnectAttempts} attempts." +
                $" Added {addedConnections} connections in {connectAttempts} attempts."
            );
        }
    }

    private IEnumerator RandomizeNodesVisuals()
    {
        while (true)
        {
            yield return new WaitForSeconds(randomizeNodesVisualsTimer);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            int randomizedNodesVisuals = 0;
            for (int i = 0; i < randomizeNodesVisualsAttempts && randomizedNodesVisuals < maxRandomizeNodesVisuals; i++)
            {
                Node node = Utils.Sample(nodes);

                if (
                    IsNodeRendered(node) ||
                    node.connections[0] != null && IsNodeRendered(node.connections[0]) ||
                    node.connections[1] != null && IsNodeRendered(node.connections[1])
                )
                    continue;

                nodeGenerator.SetNodeVisuals(node);
                randomizedNodesVisuals++;
            }

            stopwatch.Stop();
            Debug.Log(
                $"Done RandomizeNodesVisuals() in {stopwatch.Elapsed}." +
                $" Randomized {randomizedNodesVisuals} nodes in {randomizeNodesVisualsAttempts} attempts."
            );
        }
    }

    private bool BreakRandomConnection()
    {
        Node node = Utils.Sample(nodes);
        if (IsNodeRendered(node))
            return false;

        Direction direction = (Direction)Random.Range(0, 4);

        Node connected = node.GetConnected(direction);
        if (connected == null || IsNodeRendered(connected))
            return false;

        node.Disconnect(direction);

        // Ensure if graph stays interconnected
        if (node.CountAllReachableNodes() != nodes.Count)
        {
            // If not, bring this connection back
            node.Connect(direction, connected);
            return false;
        }

        return true;
    }

    private bool AddRandomConnection()
    {
        Node first = Utils.Sample(nodes);
        if (IsNodeRendered(first)) return false;

        Node second = Utils.Sample(nodes);
        if (IsNodeRendered(second)) return false;

        Direction direction = (Direction)Random.Range(0, 4);
        if (first.GetConnected(direction) != null || second.GetConnected(direction.ToOpposite()) != null)
            return false;

        first.Connect(direction, second);

        return true;
    }

    private bool IsNodeRendered(Node node)
    {
        return mazeRenderer.RenderedNodes.Any((renderedNode) => renderedNode.id == node.id);
    }
}
