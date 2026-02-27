using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleMenuController : MonoBehaviour
{
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("SimpleMenuController: GameManager.Instance is null");
            return;
        }

        PopulateMenu();
    }

    private void PopulateMenu()
    {
        List<MiniGameData> games = GameManager.Instance.miniGameList;

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var game in games)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            var texts = btnObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = game.gameName;
            if (texts.Length > 1) texts[1].text = $"Best: {game.HighScore}";

            var img = btnObj.GetComponent<Image>();
            if (img != null) img.color = game.themeColor;

            var btn = btnObj.GetComponent<Button>();
            var capturedGame = game;
            btn.onClick.AddListener(() => GameManager.Instance.LoadMiniGame(capturedGame));
        }
    }
}
