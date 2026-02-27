using UnityEngine;

[CreateAssetMenu(fileName = "NewMiniGame", menuName = "MiniGames/MiniGameData")]
public class MiniGameData : ScriptableObject
{
    public string gameName;
    public string sceneName;
    public Sprite icon;
    public Color themeColor = Color.white;
    [TextArea] public string description;

    /// <summary>Identifiant unique pour la sauvegarde. Utilise gameName par défaut.</summary>
    public string GameId => string.IsNullOrEmpty(gameName) ? name : gameName;

    public int HighScore
    {
        get
        {
            if (SaveManager.Instance != null)
                return SaveManager.Instance.GetHighScore(GameId);
            return PlayerPrefs.GetInt($"HighScore_{GameId}", 0);
        }
        set
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.TrySaveHighScore(GameId, value);
            else
            {
                PlayerPrefs.SetInt($"HighScore_{GameId}", value);
                PlayerPrefs.Save();
            }
        }
    }
}
