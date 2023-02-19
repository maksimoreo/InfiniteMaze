using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generates single node
public class NodeGenerator : MonoBehaviour
{
    public List<WeightedPrefab> floorPrefabs;
    public List<WeightedPrefab> wallPrefabs;

    public WeightedPrefabsPicker floorPicker;
    public WeightedPrefabsPicker wallPicker;

    private IdGenerator nodeIdGenerator = new IdGenerator();

    void Awake()
    {
        floorPicker = new WeightedPrefabsPicker(floorPrefabs);
        wallPicker = new WeightedPrefabsPicker(wallPrefabs);
    }

    public Node Generate()
    {
        Node node = new Node()
        {
            id = nodeIdGenerator.Next(),
        };

        SetNodeVisuals(node);

        return node;
    }

    public void SetNodeVisuals(Node node)
    {
        node.centerFloor = PickFloor();
        node.upFloor = PickFloor();
        node.rightFloor = PickFloor();

        node.centerUpWallId = wallPicker.Pick();
        node.centerRightWallId = wallPicker.Pick();
        node.centerDownWallId = wallPicker.Pick();
        node.centerLeftWallId = wallPicker.Pick();

        node.upRightWallId = wallPicker.Pick();
        node.upLeftWallId = wallPicker.Pick();
        node.rightUpWallId = wallPicker.Pick();
        node.rightDownWallId = wallPicker.Pick();
    }

    private FloorVariant PickFloor()
    {
        return new FloorVariant()
        {
            rotation = Random.Range(0, 3),
            floorId = floorPicker.Pick(),
        };
    }
}
