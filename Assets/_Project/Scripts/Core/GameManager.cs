using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Mini Games")]
    public List<MiniGameData> miniGameList = new List<MiniGameData>();

    private int currentScore;
    public int CurrentScore
    {
        get => currentScore;
        set
        {
            currentScore = value;
            OnScoreChanged?.Invoke(currentScore);
        }
    }

    public MiniGameData CurrentMiniGame { get; set; }

    public System.Action<int> OnScoreChanged;
    public System.Action OnGameOver;

    /// <summary>
    /// Ensures GameManager + SaveManager + SceneLoader exist.
    /// Call from any scene to guarantee the singletons are live.
    /// </summary>
    public static void EnsureExists()
    {
        if (Instance != null) return;

        var go = new GameObject("[GameManager]");
        go.AddComponent<GameManager>();

        // Also ensure SaveManager
        if (SaveManager.Instance == null)
        {
            var saveGO = new GameObject("[SaveManager]");
            saveGO.AddComponent<SaveManager>();
        }

        // Also ensure SceneLoader
        if (SceneLoader.Instance == null)
        {
            var slGO = new GameObject("[SceneLoader]");
            slGO.AddComponent<SceneLoader>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure siblings exist
        if (SaveManager.Instance == null)
        {
            var saveGO = new GameObject("[SaveManager]");
            saveGO.AddComponent<SaveManager>();
        }
        if (SceneLoader.Instance == null)
        {
            var slGO = new GameObject("[SceneLoader]");
            slGO.AddComponent<SceneLoader>();
        }
    }

    public void ResetScore()
    {
        CurrentScore = 0;
    }

    public bool TrySaveHighScore(MiniGameData game, int score)
    {
        if (score > game.HighScore)
        {
            game.HighScore = score;
            return true;
        }
        return false;
    }

    public void TriggerGameOver()
    {
        OnGameOver?.Invoke();
    }

    public void LoadMiniGame(MiniGameData game)
    {
        CurrentMiniGame = game;
        ResetScore();
        SceneLoader.Instance.LoadScene(game.sceneName);
    }

    public void ReturnToMenu()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }
}
