using UnityEngine;

[System.Serializable]
public record Configuration
{
    // General
    public float playerSpeed = 2.5f;
    public Vector3 cameraOffset = new(-0.7f, 8, -3.5f);
    public bool showOnScreenStick = false;

    // Generation
    public int maxNodesCount = 100;
    public float neighbourChance1 = 0.9f;
    public float neighbourChance2 = 0.4f;
    public float neighbourChance3 = 0.2f;
    public float neighbourChance4 = 0.5f;
    public int additionalConnectionsAttempts = 100;
    public int maxAdditionalConnections = 100;

    public float restructureMazeSeconds = 30f;
    public int newConnectionsAttempts = 50;
    public int maxNewConnections = 5;
    public int disconnectsAttempts = 100;
    public int maxDisconnects = 10;

    public float randomizeNodesVisualsSeconds = 20f;
    public int randomizeNodesVisualsAttempts = 100;
    public int maxRandomizeNodesVisuals = 50;
}

// Manages whole Application
public class GameManager : MonoBehaviour
{
    public static Configuration configuration = new();

    public bool useStaticConfiguration = true;
    public GameObject onScreenStick;
    public LevelLoader levelLoader;
    public Player player;
    public MazeRenderer mazeRenderer;
    public FiniteMazeGenerator finiteMazeGenerator;

    void Start()
    {
        // Affects only Unity Editor
        Application.targetFrameRate = 60;

        if (useStaticConfiguration)
        {
            ApplyConfiguration();
        }
        mazeRenderer.RenderFirstTime();
    }

    public void ReloadScene()
    {
        levelLoader.ReloadScene();
    }

    #region JS API
    public void EnableOnScreenStick()
    {
        onScreenStick.SetActive(true);
        configuration.showOnScreenStick = true;
    }

    public void DisableOnScreenStick()
    {
        onScreenStick.SetActive(false);
        configuration.showOnScreenStick = false;
    }

    public void SetConfiguration(string jsonConfiguration)
    {
        configuration = JsonUtility.FromJson<Configuration>(jsonConfiguration);
    }
    #endregion

    private void ApplyConfiguration()
    {
        onScreenStick.SetActive(configuration.showOnScreenStick);
        player.speed = configuration.playerSpeed;
        player.cameraOffset = configuration.cameraOffset;

        finiteMazeGenerator.maxNodeCount = configuration.maxNodesCount;
        finiteMazeGenerator.neighbourProbabilities = new float[4]
        {
            configuration.neighbourChance1,
            configuration.neighbourChance2,
            configuration.neighbourChance3,
            configuration.neighbourChance4,
        };
        finiteMazeGenerator.additionalConnectionsAttempts = configuration.additionalConnectionsAttempts;
        finiteMazeGenerator.maxAdditionalConnections = configuration.maxAdditionalConnections;

        finiteMazeGenerator.restructureMazeTimer = configuration.restructureMazeSeconds;
        finiteMazeGenerator.connectAttempts = configuration.newConnectionsAttempts;
        finiteMazeGenerator.maxConnects = configuration.maxNewConnections;
        finiteMazeGenerator.disconnectAttempts = configuration.disconnectsAttempts;
        finiteMazeGenerator.maxDisconnects = configuration.maxDisconnects;

        finiteMazeGenerator.randomizeNodesVisualsTimer = configuration.randomizeNodesVisualsSeconds;
        finiteMazeGenerator.randomizeNodesVisualsAttempts = configuration.randomizeNodesVisualsAttempts;
        finiteMazeGenerator.maxRandomizeNodesVisuals = configuration.maxRandomizeNodesVisuals;
    }
}
