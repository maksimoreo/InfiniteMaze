using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Serializes Node to JSON
public class MazeSerializer
{
    public static string Serialize(Node node)
    {
        return new MazeSerializer(node).Serialize();
    }

    private Node root;

    public MazeSerializer(Node root)
    {
        this.root = root;
    }

    public string Serialize()
    {
        JObject j = new JObject();

        JArray vertices = new JArray();
        JArray edges = new JArray();

        // BFS
        Queue<Node> queue = new Queue<Node>();
        HashSet<int> visitedNodesIds = new HashSet<int>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            Node node = queue.Dequeue();
            visitedNodesIds.Add(node.id);

            for (int i = 0; i < node.connections.Length; i++)
            {
                Node connected = node.connections[i];

                if (connected != null && !visitedNodesIds.Contains(connected.id))
                {
                    queue.Enqueue(connected);

                    // yield
                    edges.Add(JObject.FromObject(new
                    {
                        first = node.id,
                        second = connected.id,
                        direction = i,
                    }));
                }
            }

            // yield
            vertices.Add(SerializeNode(node));
        }

        j["vertices"] = vertices;
        j["edges"] = edges;

        return j.ToString();
    }

    private JObject SerializeNode(Node node)
    {
        return JObject.FromObject(new
        {
            id = node.id,
            generated = node.generated,
            centerUpWallId = node.centerUpWallId,
            centerRightWallId = node.centerRightWallId,
            centerDownWallId = node.centerDownWallId,
            centerLeftWallId = node.centerLeftWallId,
            upRightWallId = node.upRightWallId,
            upLeftWallId = node.upLeftWallId,
            rightUpWallId = node.rightUpWallId,
            rightDownWallId = node.rightDownWallId,

            centerFloor = JToken.FromObject(node.centerFloor),
            upFloor = JToken.FromObject(node.upFloor),
            rightFloor = JToken.FromObject(node.rightFloor),
        });
    }
}
