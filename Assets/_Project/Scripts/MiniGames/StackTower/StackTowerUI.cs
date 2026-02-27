using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StackTowerUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI perfectText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("References")]
    [SerializeField] private StackTowerManager towerManager;

    private float perfectFadeTimer;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (perfectText != null)
            perfectText.gameObject.SetActive(false);

        if (towerManager != null)
        {
            towerManager.OnScoreUpdated += UpdateScore;
            towerManager.OnPerfect += ShowPerfect;
            towerManager.OnGameOver += ShowGameOver;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenu);

        UpdateScore(0);
    }

    private void Update()
    {
        if (perfectText != null && perfectText.gameObject.activeSelf)
        {
            perfectFadeTimer -= Time.deltaTime;
            if (perfectFadeTimer <= 0f)
                perfectText.gameObject.SetActive(false);
            else
            {
                Color c = perfectText.color;
                c.a = Mathf.Clamp01(perfectFadeTimer / 0.5f);
                perfectText.color = c;

                float scale = 1f + (1f - perfectFadeTimer) * 0.3f;
                perfectText.transform.localScale = Vector3.one * scale;
            }
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private void ShowPerfect(bool isPerfect)
    {
        if (!isPerfect || perfectText == null) return;

        perfectText.gameObject.SetActive(true);
        perfectText.text = "PERFECT!";
        Color c = perfectText.color;
        c.a = 1f;
        perfectText.color = c;
        perfectText.transform.localScale = Vector3.one;
        perfectFadeTimer = 1f;
    }

    private void ShowGameOver(int score, int highScore)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = $"Score: {score}";

        if (highScoreText != null)
            highScoreText.text = $"Best: {highScore}";
    }

    private void OnRestart()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (towerManager != null)
            towerManager.StartGame();
    }

    private void OnMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        if (towerManager != null)
        {
            towerManager.OnScoreUpdated -= UpdateScore;
            towerManager.OnPerfect -= ShowPerfect;
            towerManager.OnGameOver -= ShowGameOver;
        }
    }
}
