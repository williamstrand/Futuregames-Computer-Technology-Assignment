using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    [Header("Health Bar")]
    [SerializeField] Image healthSpritePrefab;
    [SerializeField] Transform healthBar;
    Image[] healthSprites;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TextMeshProUGUI gameOverScoreText;


    public void Initialize(PlayerController player, GameController gameController)
    {
        player.OnHealthChange += UpdateHealthBar;
        gameController.OnScoreUpdated += UpdateScore;
        gameController.OnGameOver += GameOver;

        gameOverPanel.SetActive(false);
        CreateHealthBar(player.Hp);
    }

    void CreateHealthBar(int maxHealth)
    {
        healthSprites = new Image[maxHealth];

        for (var i = 0; i < maxHealth; i++)
        {
            healthSprites[i] = Instantiate(healthSpritePrefab, healthBar);
        }
    }

    void UpdateHealthBar(int health)
    {
        for (var i = 0; i < healthSprites.Length; i++)
        {
            healthSprites[i].enabled = i < health;
        }
    }

    void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }

    void GameOver(int score)
    {
        healthBar.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = $"Final Score\n{score}";
    }
}
