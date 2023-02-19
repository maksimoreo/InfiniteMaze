using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Provides handly actions for development
public class DevUI : MonoBehaviour
{
    public static string MAZE_SAVE_FILE = "temp/maze.output.json";
    public static string MAZE_LOAD_FILE = "temp/maze.input.json";

    public GameManager gameManager;
    public MazeRenderer mazeRenderer;
    public Player player;

    private string mazeSaveFile = MAZE_SAVE_FILE;
    private string mazeLoadFile = MAZE_LOAD_FILE;
    private int loadFilesSelectedIndex = 0;
    private string[] loadFiles = new string[] {
        "temp/maze.input.json",
        "temp/maze.output.json",
        "Assets/SavedMazes/singleCell.jsonc",
        "Assets/SavedMazes/singleCellInfinite.jsonc",
        "Assets/SavedMazes/shortAndLongHallways.jsonc",
    };

    public void OnGUI()
    {
        if (GUI.Button(new Rect(10, 150, 90, 20), "Generate"))
            GenerateMaze();

        if (GUI.Button(new Rect(100, 150, 90, 20), "Reload"))
            gameManager.ReloadScene();

        mazeSaveFile = GUI.TextField(new Rect(10, 200, 190, 20), mazeSaveFile);
        if (GUI.Button(new Rect(200, 200, 90, 20), "Save"))
            SaveMaze();

        mazeLoadFile = GUI.TextField(new Rect(10, 250, 190, 20), mazeLoadFile);
        if (GUI.Button(new Rect(200, 250, 90, 20), "Load"))
            LoadMaze(mazeLoadFile);

        loadFilesSelectedIndex = GUI.SelectionGrid(new Rect(10, 300, 190, loadFiles.Length * 20), loadFilesSelectedIndex, loadFiles, 1);
        if (GUI.Button(new Rect(200, 300, 90, 20), "Load"))
            LoadMaze(loadFiles[loadFilesSelectedIndex]);
    }

    private async void SaveMaze()
    {
        Debug.Log("SaveMaze(): Start");

        string json_string = MazeSerializer.Serialize(mazeRenderer.root);
        await System.IO.File.WriteAllTextAsync(mazeSaveFile, json_string);

        Debug.Log("SaveMaze(): Done");
    }

    private async void LoadMaze(string filename)
    {
        Debug.Log("LoadMaze(): Start");

        string json_string = await System.IO.File.ReadAllTextAsync(filename);
        mazeRenderer.root = MazeDeserializer.Deserialize(json_string);
        player.ResetPosition();

        Debug.Log("LoadMaze(): Done");
    }

    private void GenerateMaze()
    {
        Debug.Log("GenerateMaze(): Start");

        mazeRenderer.root = mazeRenderer.mazeGenerator.GenerateRoot();
        player.ResetPosition();

        Debug.Log("GenerateMaze(): Done");
    }
}
