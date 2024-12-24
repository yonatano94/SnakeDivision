using Zenject;
using UnityEngine;

public class ScoreSystem
{
    private int currentScore = 0;
    [Inject] private UIManager uiManager;
    [Inject] private GameSettings settings;

    public void AddScore()
    {
        currentScore += settings.PointsPerItem;
        RefreshUI();
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        RefreshUI();
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void SetScore(int score)
    {
        currentScore = score;
        RefreshUI();
    }

    private void RefreshUI()
    {
        uiManager.UpdateScore(currentScore);
    }
}