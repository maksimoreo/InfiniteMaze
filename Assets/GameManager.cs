using UnityEngine;

// Manages whole Application
public class GameManager : MonoBehaviour
{
    public GameObject onScreenStick;
    public LevelLoader levelLoader;

    void Start()
    {
        // Affects only Unity Editor
        Application.targetFrameRate = 60;
    }

    public void ReloadScene()
    {
        levelLoader.ReloadScene();
    }

    public void EnableOnScreenStick()
    {
        onScreenStick.SetActive(true);
    }

    public void DisableOnScreenStick()
    {
        onScreenStick.SetActive(false);
    }
}
