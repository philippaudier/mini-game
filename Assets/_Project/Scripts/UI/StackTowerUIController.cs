using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StackTowerUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StackTowerManager towerManager;

    private Label scoreLabel;
    private Label perfectLabel;
    private VisualElement gameOverPanel;
    private Label finalScoreLabel;
    private Label highScoreLabel;
    private VisualElement restartButton;
    private VisualElement menuButton;

    private float perfectTimer;
    private bool perfectVisible;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;

        scoreLabel = root.Q<Label>("score-label");
        perfectLabel = root.Q<Label>("perfect-label");
        gameOverPanel = root.Q("game-over-panel");
        finalScoreLabel = root.Q<Label>("final-score");
        highScoreLabel = root.Q<Label>("high-score");
        restartButton = root.Q("restart-button");
        menuButton = root.Q("menu-button");

        // Ensure game over is hidden
        gameOverPanel?.AddToClassList("hidden");

        // Button callbacks
        restartButton?.RegisterCallback<ClickEvent>(evt => OnRestart());
        menuButton?.RegisterCallback<ClickEvent>(evt => OnMenu());

        // Subscribe to game events
        if (towerManager != null)
        {
            towerManager.OnScoreUpdated += UpdateScore;
            towerManager.OnPerfect += ShowPerfect;
            towerManager.OnGameOver += ShowGameOver;
        }

        UpdateScore(0);
    }

    private void Update()
    {
        if (perfectVisible)
        {
            perfectTimer -= Time.deltaTime;
            if (perfectTimer <= 0f)
            {
                perfectVisible = false;
                perfectLabel?.RemoveFromClassList("visible");
            }
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreLabel != null)
            scoreLabel.text = score.ToString();
    }

    private void ShowPerfect(bool isPerfect)
    {
        if (!isPerfect || perfectLabel == null) return;

        perfectLabel.text = "PERFECT!";
        perfectLabel.AddToClassList("visible");
        perfectVisible = true;
        perfectTimer = 1f;
    }

    private void ShowGameOver(int score, int highScore)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.RemoveFromClassList("hidden");

        if (finalScoreLabel != null)
            finalScoreLabel.text = $"Score: {score}";

        if (highScoreLabel != null)
            highScoreLabel.text = $"Best: {highScore}";
    }

    private void OnRestart()
    {
        gameOverPanel?.AddToClassList("hidden");

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
