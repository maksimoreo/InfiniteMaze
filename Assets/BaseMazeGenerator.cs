using UnityEngine;

public abstract class BaseMazeGenerator : MonoBehaviour
{
    public abstract Node GenerateRoot();
    public abstract void GenerateNeighbours(Node node);
}
