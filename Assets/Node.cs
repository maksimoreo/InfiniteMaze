using System.Collections.Generic;

public record FloorVariant
{
    public static float[] ROTATION_TO_ANGLE_MAP = { 0, 90, 180, 270 };
    public int floorId;
    public int rotation;
}

public record Node
{
    public int id;

    public FloorVariant centerFloor;
    public FloorVariant upFloor;
    public FloorVariant rightFloor;

    public int centerUpWallId;
    public int centerRightWallId;
    public int centerDownWallId;
    public int centerLeftWallId;

    public int upRightWallId;
    public int upLeftWallId;

    public int rightUpWallId;
    public int rightDownWallId;

    public Node[] connections = new Node[4];

    public bool generated = false;
    public bool isExit = false;

    public Node GetConnected(Direction direction) => connections[(int)direction];
    public void SetConnected(Direction direction, Node connected) => connections[(int)direction] = connected;

    public void Connect(Direction direction, Node other)
    {
        Direction oppositeDirection = direction.ToOpposite();

        if (GetConnected(direction) != null) throw new System.ArgumentException("Node already exists at " + direction.ToString());
        if (other.GetConnected(oppositeDirection) != null) throw new System.ArgumentException("Node already exists for other node at " + oppositeDirection.ToString());

        SetConnected(direction, other);
        other.SetConnected(oppositeDirection, this);

        // TODO: Allow chain syntax:
        // node1
        //   .Connect(node2, Direction.UP)
        //   .Connect(node3, Direction.RIGHT)
        //   ...
        // return this;
    }

    public void Disconnect(Direction direction)
    {
        Node connected = GetConnected(direction);
        SetConnected(direction, null);
        connected.SetConnected(direction.ToOpposite(), null);
    }

    public int GetCenterWallId(int rotation)
    {
        switch (rotation)
        {
            case 0: return centerUpWallId;
            case 1: return centerRightWallId;
            case 2: return centerDownWallId;
            case 3: return centerLeftWallId;
            default: throw new System.ArgumentException("Invalid rotation: " + rotation);
        }
    }

    public int CountAllReachableNodes()
    {
        HashSet<int> visitedNodesIds = new HashSet<int>();
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(this);

        while (queue.Count != 0)
        {
            Node node = queue.Dequeue();
            visitedNodesIds.Add(node.id);

            foreach (Node connected in node.connections)
                if (connected != null && !visitedNodesIds.Contains(connected.id))
                    queue.Enqueue(connected);
        }

        return visitedNodesIds.Count;
    }
}
