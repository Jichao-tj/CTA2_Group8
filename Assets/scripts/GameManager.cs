using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int totalEnemies = 0;
    int defeatedEnemies = 0;

    bool gameEnded = false;

    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;
    [SerializeField] TextMeshProUGUI enemyProgressText;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy()
    {
        totalEnemies++;
        UpdateEnemyProgress();
    }

    public void EnemyDefeated()
    {
        defeatedEnemies++;
        UpdateEnemyProgress();

        if (defeatedEnemies >= totalEnemies && !gameEnded)
        {
            WinGame();
        }
    }

    void UpdateEnemyProgress()
    {
        enemyProgressText.text = $"Enemies Defeated: {defeatedEnemies}/{totalEnemies}";
    }

    public void PlayerDied()
    {
        if (!gameEnded)
        {
            LoseGame();
        }
    }

    void WinGame()
    {
        gameEnded = true;
        winScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    void LoseGame()
    {
        gameEnded = true;
        loseScreen.SetActive(true);
        Time.timeScale = 0f;
    }
}
