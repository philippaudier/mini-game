using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameCard : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button playButton;
    [SerializeField] private Image backgroundImage;

    private MiniGameData gameData;
    private bool isCentered;

    public void Setup(MiniGameData data)
    {
        gameData = data;

        if (titleText != null)
            titleText.text = data.gameName;

        if (highScoreText != null)
            highScoreText.text = $"Best: {data.HighScore}";

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (backgroundImage != null)
            backgroundImage.color = data.themeColor;

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        // Also make the whole card tappable
        Button cardButton = GetComponent<Button>();
        if (cardButton == null)
        {
            cardButton = gameObject.AddComponent<Button>();
            cardButton.transition = Selectable.Transition.None;
        }
        cardButton.onClick.AddListener(OnPlayClicked);
    }

    public void SetCentered(bool centered, bool force = false)
    {
        if (!force && isCentered == centered) return;
        isCentered = centered;

        if (playButton != null)
            playButton.interactable = centered;

        // Also update the card-level button
        Button cardButton = GetComponent<Button>();
        if (cardButton != null && cardButton != playButton)
            cardButton.interactable = centered;
    }

    private void OnPlayClicked()
    {
        if (!isCentered) return;

        if (gameData == null)
        {
            Debug.LogWarning("MiniGameCard: gameData is null");
            return;
        }
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("MiniGameCard: GameManager.Instance is null");
            return;
        }

        Debug.Log($"Loading mini game: {gameData.gameName} (scene: {gameData.sceneName})");
        GameManager.Instance.LoadMiniGame(gameData);
    }

    private void OnEnable()
    {
        if (gameData != null && highScoreText != null)
            highScoreText.text = $"Best: {gameData.HighScore}";
    }
}
