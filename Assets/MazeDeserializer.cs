using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Deserializes JSON to Graph. Returns Node
public class MazeDeserializer
{
    public static Node Deserialize(string json)
    {
        return new MazeDeserializer(json).Deserialize();
    }

    private string json;
    private Node root;
    private Dictionary<int, Node> idToNodeMap = new Dictionary<int, Node>();

    public MazeDeserializer(string json)
    {
        this.json = json;
    }

    public Node Deserialize()
    {
        JObject j = JObject.Parse(json);

        // Nodes
        JArray jVertices = (JArray)j["vertices"];

        // First node is root
        root = DeserializeNode((JObject)jVertices[0]);
        idToNodeMap.Add(root.id, root);

        // Other nodes
        for (int i = 1; i < jVertices.Count; i++)
        {
            Node node = DeserializeNode((JObject)jVertices[i]);
            idToNodeMap.Add(node.id, node);
        }

        // Node connections
        JArray jEdges = (JArray)j["edges"];
        for (int i = 0; i < jEdges.Count; i++)
        {
            JObject jEdge = (JObject)jEdges[i];

            Node first = idToNodeMap[(int)jEdge["first"]];
            Node second = idToNodeMap[(int)jEdge["second"]];
            Direction direction = (Direction)(int)jEdge["direction"];

            first.Connect(direction, second);
        }

        return root;
    }

    private Node DeserializeNode(JObject j)
    {
        Node node = new Node();

        node.id = (int)j["id"];
        node.generated = JFetch(j, "generated", true);
        node.centerUpWallId = JFetchWallId(j, "centerUpWallId");
        node.centerRightWallId = JFetchWallId(j, "centerRightWallId");
        node.centerDownWallId = JFetchWallId(j, "centerDownWallId");
        node.centerLeftWallId = JFetchWallId(j, "centerLeftWallId");
        node.upRightWallId = JFetchWallId(j, "upRightWallId");
        node.upLeftWallId = JFetchWallId(j, "upLeftWallId");
        node.rightUpWallId = JFetchWallId(j, "rightUpWallId");
        node.rightDownWallId = JFetchWallId(j, "rightDownWallId");

        node.centerFloor = JFetchFloorVariant(j, "centerFloor");
        node.upFloor = JFetchFloorVariant(j, "upFloor");
        node.rightFloor = JFetchFloorVariant(j, "rightFloor");

        return node;
    }

    private int JFetchWallId(JObject jObject, string key)
    {
        return JFetch(jObject, key, 0);
    }

    private int JFetch(JObject jObject, string key, int defaultValue)
    {
        if (jObject.ContainsKey(key))
            return (int)jObject[key];

        return defaultValue;
    }

    private bool JFetch(JObject jObject, string key, bool defaultValue)
    {
        if (jObject.ContainsKey(key))
            return (bool)jObject[key];

        return defaultValue;
    }

    private FloorVariant JFetchFloorVariant(JObject jObject, string key)
    {
        if (jObject.ContainsKey(key))
            return jObject[key].ToObject<FloorVariant>();

        return new FloorVariant() { floorId = 0, rotation = 0 };
    }
}
