using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    
    private float matchStartTime;
    private int currentScore = 0;
    private bool isGameOver = false;

    private void Start()
    {
        ResetTimer();
        UpdateScoreDisplay();
    }

    private void Update()
    {
        if (!isGameOver)
        {
            UpdateTimer();
        }
    }

    public void ResetTimer()
    {
        matchStartTime = Time.time;
        isGameOver = false;
    }

    private void UpdateTimer()
    {
        float currentTime = Time.time - matchStartTime;
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        scoreText.text = $"Score: {currentScore}";
    }

    public void EndGame()
    {
        isGameOver = true;
    }
}