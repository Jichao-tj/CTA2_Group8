using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int totalEnemies = 0;
    int defeatedEnemies = 0;

    bool gameEnded = false;

    float maxHealth;
    float currentHealth;

    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;
    [SerializeField] TextMeshProUGUI enemyProgressText;
    [SerializeField] Image fillImage;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

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

    public void SetInitialHealth(int value)
    {
        maxHealth = value;
        currentHealth = maxHealth;
        fillImage.fillAmount = currentHealth/maxHealth;
    }

    public void UpdateHealth(int value)
    {
        currentHealth = value;
        fillImage.fillAmount = currentHealth/maxHealth;
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
