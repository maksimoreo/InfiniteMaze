using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [Header("General")]

    [Tooltip("How many cells to render from player")]
    [Min(1)]
    public int renderDistance = 5;

    public GameManager gameManager;

    public BaseMazeGenerator mazeGenerator;
    public NodeGenerator nodeGenerator;

    [Header("Objects")]

    public Transform floorsContainer;
    public GameObject exitFloorPrefab;

    public Transform wallFacesContainer;

    public Transform flatWallsContainer;
    public GameObject flatWallPrefab;

    public Transform shadowCubesContainer;
    public GameObject shadowCubePrefab;

    public System.Collections.ObjectModel.ReadOnlyCollection<Node> RenderedNodes { get => renderedNodes.AsReadOnly(); }

    // Changes when player is moving
    public Node root;

    private bool exitTriggered = false;

    private List<Node> renderedNodes = new List<Node>();

    public void RenderFirstTime()
    {
        root = mazeGenerator.GenerateRoot();

        RenderMaze(Vector2.zero, Vector2Int.zero);
    }

    public void OnPlayerPositionChanged(Vector2 previousRealPosition, Vector2 currentRealPosition)
    {
        Vector2Int previousHalfCellPosition = RealPositionToHalfCellPosition(previousRealPosition);
        Vector2Int currentHalfCellPosition = RealPositionToHalfCellPosition(currentRealPosition);

        if (currentHalfCellPosition == previousHalfCellPosition) return;

        Vector2Int previousNodePosition = RealPositionToNodePosition(previousRealPosition);
        Vector2Int currentNodePosition = RealPositionToNodePosition(currentRealPosition);

        if (previousNodePosition != currentNodePosition)
        {
            // Debug.Log("Changed node position from " + previousNodePosition + " to " + currentNodePosition);

            Direction nodeMoveDirection = DirectionUtils.FromVector2Int(currentNodePosition - previousNodePosition);
            root = root.connections[(int)nodeMoveDirection];

            // Could happen if moving diagonally within a single frame, though physical walls should prevent this from happening
            if (root == null)
                throw new System.ArgumentException("Cannot go " + nodeMoveDirection.ToString() + ", there is no connection!");
        }

        renderedNodes.Clear();

        DestroyChildren(floorsContainer);
        DestroyChildren(flatWallsContainer);
        DestroyChildren(shadowCubesContainer);
        DestroyChildren(wallFacesContainer);

        Vector2Int currentCellPosition = RealPositionToCellPosition(currentRealPosition);
        RenderMaze(currentRealPosition, currentCellPosition);

        if (root.isExit && !exitTriggered && IsNodeCenter(currentCellPosition))
        {
            gameManager.ReloadScene();
            exitTriggered = true;
        }
    }

    private Vector2Int RealPositionToCellPosition(Vector2 v)
    {
        return new Vector2Int(
            Mathf.FloorToInt(v.x),
            Mathf.FloorToInt(v.y)
        );
    }

    private Vector2Int RealPositionToNodePosition(Vector2 v)
    {
        return new Vector2Int(
            Mathf.FloorToInt(v.x / 2f),
            Mathf.FloorToInt(v.y / 2f)
        );
    }

    private Vector2Int RealPositionToHalfCellPosition(Vector2 v)
    {
        return new Vector2Int(
            Mathf.FloorToInt(v.x * 2),
            Mathf.FloorToInt(v.y * 2)
        );
    }

    private bool IsNodeCenter(Vector2Int cellPosition)
    {
        return cellPosition.x % 2 == 0 && cellPosition.y % 2 == 0;
    }

    // ;-;
    private void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
            GameObject.Destroy(child.gameObject);
    }

    private void RenderMaze(Vector2 realPosition, Vector2Int cellPosition)
    {
        // Debug.Log("Render");

        if (cellPosition.x % 2 == 0 && cellPosition.y % 2 == 0)
        {
            // Player is standing on the node cell

            RenderNodeCenterCell(root, cellPosition);

            for (int i = 0; i < 4; i++)
            {
                Direction direction = (Direction)i;

                RenderDirectionTwoSidesSkipFirst(root, cellPosition, direction);

                Vector2Int directionCellPosition = cellPosition.Advance(direction);
                if (root.connections[i] == null)
                    PutFlatWall(directionCellPosition);

                Vector2Int diagonalCellPosition = directionCellPosition.Advance(direction.ToRight());
                PutFlatWall(diagonalCellPosition);
                PutShadowCube(diagonalCellPosition);
            }
        }
        else
        {
            // Player is standing between the node cells

            if (cellPosition.x % 2 == 0)
            {
                // Player is standing between two vertical nodes (cellPosition.y % 2 == 1)

                RenderNodeAdjacentCell(root, cellPosition, Direction.Up);
                RenderBetweenNodes(root, cellPosition, Direction.Up, isAtLeftPart(realPosition.y));
            }
            else
            {
                // Player is standing between two horizontal nodes (cellPosition.x % 2 == 1)

                RenderNodeAdjacentCell(root, cellPosition, Direction.Right);
                RenderBetweenNodes(root, cellPosition, Direction.Right, isAtLeftPart(realPosition.x));
            }
        }
    }

    // Returns true if coordinate is closer to left (or down) integer part
    // isAtLeftPart(0.1) == True
    // isAtLeftPart(0.9) == False
    // isAtLeftPart(10.1) == True
    // isAtLeftPart(10.9) == False
    // isAtLeftPart(-0.1) == False
    // isAtLeftPart(-0.9) == True
    // isAtLeftPart(-10.1) == False
    // isAtLeftPart(-10.9) == True
    // Based on https://stackoverflow.com/a/1082938
    private bool isAtLeftPart(float x)
    {
        return (x % 1 + 1) % 1 <= 0.5;
    }

    private void RenderBetweenNodes(Node node, Vector2Int cellPosition, Direction direction, bool closerToPrimaryNode)
    {
        Direction oppositeDirection = direction.ToOpposite();
        Direction rightDirection = direction.ToRight();
        Direction leftDirection = direction.ToLeft();

        // Primary node tunnels
        Vector2Int primaryNodePosition = cellPosition.Advance(oppositeDirection);

        if (closerToPrimaryNode)
        {
            RenderDirectionTwoSides(node, primaryNodePosition, rightDirection);
            RenderDirectionTwoSides(node, primaryNodePosition, leftDirection);
        }
        else
        {
            RenderDirectionSingleSide(node, primaryNodePosition, rightDirection, oppositeDirection);
            RenderDirectionSingleSide(node, primaryNodePosition, leftDirection, oppositeDirection);
        }

        // Connected node tunnels
        Node connectedNode = node.connections[(int)direction];
        Vector2Int connectedNodePosition = cellPosition.Advance(direction);

        if (closerToPrimaryNode)
        {
            RenderDirectionSingleSide(connectedNode, connectedNodePosition, rightDirection, direction);
            RenderDirectionSingleSide(connectedNode, connectedNodePosition, leftDirection, direction);
        }
        else
        {
            RenderDirectionTwoSides(connectedNode, connectedNodePosition, rightDirection);
            RenderDirectionTwoSides(connectedNode, connectedNodePosition, leftDirection);
        }

        // Down & Up vertical tunnels
        RenderDirectionTwoSidesSkipFirst(node, primaryNodePosition, oppositeDirection);
        RenderDirectionTwoSidesSkipFirst(connectedNode, connectedNodePosition, direction);

        RenderNodeCenterCell(node, primaryNodePosition);
        RenderNodeCenterCell(connectedNode, connectedNodePosition);

        // Walls
        Vector2Int leftPosition = cellPosition.Advance(leftDirection);
        PutFlatWall(leftPosition);

        Vector2Int rightPosition = cellPosition.Advance(rightDirection);
        PutFlatWall(rightPosition);

        if (node.connections[(int)rightDirection] == null)
            PutFlatWall(primaryNodePosition.Advance(rightDirection));
        if (node.connections[(int)leftDirection] == null)
            PutFlatWall(primaryNodePosition.Advance(leftDirection));
        if (connectedNode.connections[(int)rightDirection] == null)
            PutFlatWall(connectedNodePosition.Advance(rightDirection));
        if (connectedNode.connections[(int)leftDirection] == null)
            PutFlatWall(connectedNodePosition.Advance(leftDirection));
    }

    // Same as RenderDirectionTwoSides, but with special first iteration
    private void RenderDirectionTwoSidesSkipFirst(Node node, Vector2Int position, Direction direction)
    {
        Direction rightDirection = direction.ToRight();
        Direction leftDirection = direction.ToLeft();

        Node previousNode = node;
        node = node.GetConnected(direction);

        Vector2Int positionBetweenNodes = position.Advance(direction);

        if (node == null)
        {
            PutShadowCube(positionBetweenNodes);
            return;
        }

        mazeGenerator.GenerateNeighbours(node);

        RenderNodeAdjacentCell(previousNode, positionBetweenNodes, direction);

        position = positionBetweenNodes.Advance(direction);

        RenderSideOfNode(node, position, rightDirection);
        RenderSideOfNode(node, position, leftDirection);
        RenderNodeCenterCell(node, position);

        RenderDirectionTwoSides(node, position, direction);
    }

    private void RenderDirectionTwoSides(Node node, Vector2Int position, Direction direction)
    {
        Direction rightDirection = direction.ToRight();
        Direction leftDirection = direction.ToLeft();

        for (int i = 0; i < renderDistance; i++)
        {
            Node previousNode = node;
            node = node.GetConnected(direction);

            Vector2Int positionBetweenNodes = position.Advance(direction);
            PutShadowCube(positionBetweenNodes.Advance(rightDirection));
            PutShadowCube(positionBetweenNodes.Advance(leftDirection));

            if (node == null)
            {
                PutShadowCube(positionBetweenNodes);
                return;
            }

            mazeGenerator.GenerateNeighbours(node);

            RenderNodeAdjacentCell(previousNode, positionBetweenNodes, direction);

            position = positionBetweenNodes.Advance(direction);

            RenderSideOfNode(node, position, rightDirection);
            RenderSideOfNode(node, position, leftDirection);
            RenderNodeCenterCell(node, position);
        }
    }

    private void RenderDirectionSingleSide(Node node, Vector2Int position, Direction direction, Direction sideDirection)
    {
        for (int i = 0; i < renderDistance; i++)
        {
            Node previousNode = node;
            node = node.GetConnected(direction);

            Vector2Int positionBetweenNodes = position.Advance(direction);
            PutShadowCube(positionBetweenNodes.Advance(sideDirection));

            if (node == null)
            {
                PutShadowCube(positionBetweenNodes);
                return;
            }

            mazeGenerator.GenerateNeighbours(node);

            RenderNodeAdjacentCell(previousNode, positionBetweenNodes, direction);

            position = positionBetweenNodes.Advance(direction);

            RenderSideOfNode(node, position, sideDirection);
            RenderNodeCenterCell(node, position);
        }
    }

    private void RenderSideOfNode(Node node, Vector2Int cellPosition, Direction sideDirection)
    {
        Vector2Int sidePosition = cellPosition.Advance(sideDirection);

        if (node.connections[(int)sideDirection] == null)
            PutShadowCube(sidePosition);
        else
            RenderNodeAdjacentCell(node, sidePosition, sideDirection);
    }

    private void RenderNodeCenterCell(Node node, Vector2Int cellPosition)
    {
        if (node.isExit)
            Instantiate(exitFloorPrefab, new Vector3(cellPosition.x, 0, cellPosition.y), Quaternion.identity, floorsContainer);
        else
            PutFloor(cellPosition, node.centerFloor);

        for (int rotation = 0; rotation < 4; rotation++)
            if (node.connections[rotation] == null)
                PutWallFace(cellPosition, node.GetCenterWallId(rotation), rotation);

        renderedNodes.Add(node);
    }

    private void RenderNodeAdjacentCell(Node node, Vector2Int cellPosition, Direction direction)
    {
        (node, direction) = Normalize(node, direction);

        if (direction == Direction.Up)
        {
            PutFloor(cellPosition, node.upFloor);
            PutWallFace(cellPosition, node.upRightWallId, 1);
            PutWallFace(cellPosition, node.upLeftWallId, 3);
        }
        else
        {
            PutFloor(cellPosition, node.rightFloor);
            PutWallFace(cellPosition, node.rightUpWallId, 0);
            PutWallFace(cellPosition, node.rightDownWallId, 2);
        }
    }

    // Assumes node has connections at direction
    private (Node, Direction) Normalize(Node node, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
            case Direction.Right:
                return (node, direction);
            case Direction.Down:
            case Direction.Left:
                return (node.GetConnected(direction), direction.ToOpposite());
            default:
                throw new System.ArgumentException("Undefined direction: " + direction);
        }
    }

    private void PutFloor(Vector2Int position, FloorVariant variant)
    {
        GameObject prefab = nodeGenerator.floorPicker.GetPrefab(variant.floorId);
        Quaternion rotation = Quaternion.Euler(0, FloorVariant.ROTATION_TO_ANGLE_MAP[variant.rotation], 0);
        Instantiate(prefab, new Vector3(position.x, 0, position.y), rotation, floorsContainer);
    }

    private void PutFlatWall(Vector2Int position)
    {
        Instantiate(flatWallPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity, flatWallsContainer);
    }

    private void PutShadowCube(Vector2Int position)
    {
        Instantiate(shadowCubePrefab, new Vector3(position.x, 0, position.y), Quaternion.identity, shadowCubesContainer);
    }

    private void PutWallFace(Vector2Int position, int variantId, int rotationId)
    {
        GameObject prefab = nodeGenerator.wallPicker.GetPrefab(variantId);
        Quaternion rotation = Quaternion.Euler(0, FloorVariant.ROTATION_TO_ANGLE_MAP[rotationId], 0);
        Instantiate(prefab, new Vector3(position.x, 0, position.y), rotation, wallFacesContainer);
    }
}
