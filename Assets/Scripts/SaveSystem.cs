using Zenject;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[Serializable]
public class GameSaveData
{
    public List<SerializableVector2Int> snakeBody;
    public List<SerializableVector2Int> itemPositions;
    public int score;
    public SerializableVector2Int lastDirection;
}

[Serializable]
public struct SerializableVector2Int
{
    public int x;
    public int y;

    public SerializableVector2Int(Vector2Int v)
    {
        x = v.x;
        y = v.y;
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(x, y);
    }
}

// missing namespcae
// This save system is tightly coupled to the specific game code, such as SnakeController.
public class SaveSystem : MonoBehaviour
{
    [Inject] private GameSettings settings;
    [Inject] private SnakeController snakeController;
    [Inject] private ItemManager itemManager;
    [Inject] private ScoreSystem scoreSystem;
    [Inject] private GridSystem gridSystem;
    [Inject] private InputHandler inputHandler;

    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, settings.SaveFileName);
    }

    public async void SaveGame()
    {
        try
        {
            await SaveGameAsync();
            Debug.Log("Game saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving game: {e.Message}");
        }
    }

    public async void LoadGame()
    {
        try
        {
            bool success = await LoadGameAsync();
            if (success)
            {
                Debug.Log("Game loaded successfully!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading game: {e.Message}");
        }
    }

    public async Task SaveGameAsync()
    {
        GameSaveData saveData = new GameSaveData
        {
            snakeBody = snakeController.SnakeBody.ConvertAll(v => new SerializableVector2Int(v)),
            itemPositions = itemManager.GetAllItemPositions().ConvertAll(v => new SerializableVector2Int(v)),
            score = scoreSystem.GetCurrentScore(),
            lastDirection = new SerializableVector2Int(inputHandler.GetLastDirection())
        };

        string jsonData = JsonUtility.ToJson(saveData, true);
        await File.WriteAllTextAsync(saveFilePath, jsonData);
    }

    public async Task<bool> LoadGameAsync()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("No save file found!");
            return false;
        }

        string jsonData = await File.ReadAllTextAsync(saveFilePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

        List<Vector2Int> snakeBody = saveData.snakeBody.ConvertAll(v => v.ToVector2Int());
        List<Vector2Int> itemPositions = saveData.itemPositions.ConvertAll(v => v.ToVector2Int());
        Vector2Int lastDirection = saveData.lastDirection.ToVector2Int();
        
        //why are you switching to main thread? 
        await SwitchToMainThread();

        gridSystem.Initialize();
        itemManager.Reset();
        itemManager.Initialize();

        snakeController.LoadGameState(snakeBody);
        inputHandler.SetDirection(lastDirection);
        scoreSystem.SetScore(saveData.score);

        itemManager.ClearAllItems();

        // why does the save system doing the spawnitems
        foreach (Vector2Int itemPos in itemPositions)
        {
            itemManager.SpawnItemAt(itemPos);
        }

        return true;
    }
    
    private async Task SwitchToMainThread()
    {
        //why this will switch to the main thread?
        await Task.Yield();
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    public async Task DeleteSaveFileAsync()
    {
        if (File.Exists(saveFilePath))
        {
            await Task.Run(() => File.Delete(saveFilePath));
            Debug.Log("Save file deleted!");
        }
    }

    public void DeleteSaveFile()
    {
        _ = DeleteSaveFileAsync();
    }
}