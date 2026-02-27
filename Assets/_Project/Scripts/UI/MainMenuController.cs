using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset buttonTemplate;
    [SerializeField] private UIDocument uiDocument;

    private void Start()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;
        var gameList = root.Q<ScrollView>("game-list");

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("MainMenuController: GameManager.Instance is null");
            return;
        }

        PopulateMenu(gameList, GameManager.Instance.miniGameList);
    }

    private void PopulateMenu(ScrollView gameList, List<MiniGameData> games)
    {
        gameList.Clear();

        foreach (var game in games)
        {
            VisualElement btnRoot;
            if (buttonTemplate != null)
            {
                btnRoot = buttonTemplate.Instantiate();
                // The template root is a TemplateContainer — get the actual .menu-button child
                var btn = btnRoot.Q(className: "menu-button");
                if (btn == null) btn = btnRoot;
                SetupButton(btn, game);
            }
            else
            {
                // Fallback: build button in code
                btnRoot = new VisualElement();
                btnRoot.AddToClassList("menu-button");
                var nameLabel = new Label(game.gameName);
                nameLabel.AddToClassList("game-name");
                var scoreLabel = new Label($"Best: {game.HighScore}");
                scoreLabel.AddToClassList("high-score");
                btnRoot.Add(nameLabel);
                btnRoot.Add(scoreLabel);
                SetupButton(btnRoot, game);
            }

            gameList.Add(btnRoot);
        }
    }

    private void SetupButton(VisualElement btn, MiniGameData game)
    {
        // Set text
        var nameLabel = btn.Q<Label>(className: "game-name");
        if (nameLabel != null) nameLabel.text = game.gameName;

        var scoreLabel = btn.Q<Label>(className: "high-score");
        if (scoreLabel != null) scoreLabel.text = $"Best: {game.HighScore}";

        // Set theme color as background
        btn.style.backgroundColor = new StyleColor(game.themeColor);

        // Click handler
        var capturedGame = game;
        btn.RegisterCallback<ClickEvent>(evt =>
        {
            GameManager.Instance.LoadMiniGame(capturedGame);
        });
    }
}
