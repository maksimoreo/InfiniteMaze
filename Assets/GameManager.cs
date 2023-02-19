using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages whole Application
public class GameManager : MonoBehaviour
{
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
}
