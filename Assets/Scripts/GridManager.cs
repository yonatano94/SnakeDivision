using Zenject;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//missing namespace
//name of the class has nothing to do with the grid
//this class kind of gameManger that knows every class in the game for some reason also mange the ui
//there is a lot of circular dependencies and strong coupling of this class and a lot of other class
public class GridManager : MonoBehaviour
{
    [Header("Components")]
    //why not to use inject method
    [Inject] private GridSystem gridSystem;
    [Inject] private ItemManager itemManager;
    [Inject] private SnakeController snakeController;
    [Inject] private ScoreSystem scoreSystem;
    [Inject] private InputHandler inputHandler;
    [Inject] private UIManager uiManager;
     [Inject] private SaveSystem saveSystem;


    [Header("UI")]
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPaused = false;

    private void Start()
    {
        snakeController.Initialize();
        
        if (loadGameButton != null)
        {
            loadGameButton.interactable = saveSystem.HasSaveFile();
        }
        
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.gameObject.SetActive(true);
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 0;

        ClearGameState();
    }

    private void Update()
    {
        if (mainMenuCanvas != null && !mainMenuCanvas.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            if (!isPaused)
            {
                snakeController.UpdateSnake();
            }
        }
    }

    public void StartNewGame()
    {
        ClearGameState();
        
        mainMenuCanvas.gameObject.SetActive(false);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
        
        InitializeGame();
    }

    private void ClearGameState()
    {
        if (gridSystem != null)
        {
            gridSystem.Initialize();
        }

        if (itemManager != null)
        {
            itemManager.Reset();
        }

        if (snakeController != null)
        {
            snakeController.ResetSnake();
        }

        if (scoreSystem != null)
        {
            scoreSystem.ResetScore();
        }

        if (uiManager != null)
        {
            uiManager.ResetTimer();
        }
    }

    public void InitializeGame()
    {
        gridSystem.Initialize();
        snakeController.ResetSnake();
        itemManager.Initialize();
        scoreSystem.ResetScore();

        if (uiManager != null)
        {
            uiManager.ResetTimer();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(isPaused);
        }
    }

    public async void SaveAndQuit()
    {
        await saveSystem.SaveGameAsync();
        ReturnToMainMenu();
    }

    public async void LoadSavedGame()
    {
        if (await saveSystem.LoadGameAsync())
        {
            mainMenuCanvas.gameObject.SetActive(false);
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1;
            isPaused = false;
        }
    }

    public void ReturnToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 0;

        ClearGameState();

        mainMenuCanvas.gameObject.SetActive(true);
        pauseMenuUI.SetActive(false);

        if (loadGameButton != null)
        {
            loadGameButton.interactable = saveSystem.HasSaveFile();
        }
    }

    public void QuickSave()
    {
        saveSystem.SaveGame();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}