using UnityEngine;
using UnityEngine.InputSystem;

public class StackTowerManager : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private float blockHeight = 0.3f;
    [SerializeField] private float startSize = 2f;
    [SerializeField] private float blockSpeed = 4f;
    [SerializeField] private float moveRange = 3f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraSmooth = 5f;
    private float cameraStartY;

    [Header("Visuals")]
    [SerializeField] private float hueStep = 0.08f;

    [Header("Game ID (for save)")]
    [SerializeField] private string gameId = "Stack Tower";

    private GameObject currentBlock;
    private Vector3 lastPosition;
    private Vector3 lastSize;
    private int stackCount;
    private int currentAxis;
    private float currentHue;
    private bool gameActive;
    private int comboCount;

    public System.Action<int> OnScoreUpdated;
    public System.Action<bool> OnPerfect;
    public System.Action<int, int> OnGameOver; // score, highscore

    private void Start()
    {
        // Auto-create GameManager if playing this scene directly
        GameManager.EnsureExists();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        cameraStartY = cameraTransform != null ? cameraTransform.position.y : 5f;

        StartGame();
    }

    public void StartGame()
    {
        // Clear existing blocks
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        stackCount = 0;
        currentAxis = 0;
        currentHue = Random.Range(0f, 1f);
        comboCount = 0;
        gameActive = true;

        lastSize = new Vector3(startSize, blockHeight, startSize);
        lastPosition = new Vector3(0f, 0f, 0f);

        GameObject baseBlock = CreateBlock(lastPosition, lastSize);
        baseBlock.name = "BaseBlock";

        if (GameManager.Instance != null)
            GameManager.Instance.ResetScore();

        OnScoreUpdated?.Invoke(0);
        SpawnNextBlock();
    }

    private void SpawnNextBlock()
    {
        stackCount++;
        float y = stackCount * blockHeight;

        Vector3 pos = new Vector3(lastPosition.x, y, lastPosition.z);
        currentBlock = CreateBlock(pos, lastSize);
        currentBlock.name = $"Block_{stackCount}";

        MovingBlock moving = currentBlock.AddComponent<MovingBlock>();
        moving.Init(currentAxis, blockSpeed, moveRange);
    }

    private GameObject CreateBlock(Vector3 position, Vector3 size)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.transform.SetParent(transform);
        block.transform.position = position;
        block.transform.localScale = size;

        Renderer rend = block.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.HSVToRGB(currentHue % 1f, 0.6f, 0.9f);
        rend.material = mat;
        currentHue += hueStep;

        return block;
    }

    private void Update()
    {
        if (!gameActive) return;

        bool tapped = false;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            tapped = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            tapped = true;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            tapped = true;

        if (tapped)
            PlaceBlock();

        // Camera follow
        if (cameraTransform != null)
        {
            float targetY = cameraStartY + Mathf.Max(0, (stackCount - 5) * blockHeight);
            Vector3 camPos = cameraTransform.position;
            camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * cameraSmooth);
            cameraTransform.position = camPos;
        }
    }

    private void PlaceBlock()
    {
        if (currentBlock == null) return;

        currentBlock.GetComponent<MovingBlock>().Stop();

        BlockCutter.CutResult result = BlockCutter.Cut(currentBlock, lastPosition, lastSize, currentAxis);

        if (result.missed)
        {
            gameActive = false;
            EndGame();
            return;
        }

        if (result.perfect)
        {
            comboCount++;
            OnPerfect?.Invoke(true);
        }
        else
        {
            comboCount = 0;
        }

        lastPosition = result.newPosition;
        lastSize = result.newSize;

        float minAxis = (currentAxis == 0) ? lastSize.x : lastSize.z;
        if (minAxis < 0.1f)
        {
            gameActive = false;
            EndGame();
            return;
        }

        int score = stackCount;
        if (GameManager.Instance != null)
            GameManager.Instance.CurrentScore = score;
        OnScoreUpdated?.Invoke(score);

        currentAxis = 1 - currentAxis;
        SpawnNextBlock();
    }

    private void EndGame()
    {
        int score = stackCount;
        int highScore = 0;

        // Save via SaveManager (works even without GameManager.CurrentMiniGame)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.TrySaveHighScore(gameId, score);
            highScore = SaveManager.Instance.GetHighScore(gameId);
        }

        // Also update MiniGameData if available
        if (GameManager.Instance != null)
        {
            var game = GameManager.Instance.CurrentMiniGame;
            if (game != null)
            {
                GameManager.Instance.TrySaveHighScore(game, score);
                highScore = game.HighScore;
            }
            GameManager.Instance.TriggerGameOver();
        }

        OnGameOver?.Invoke(score, highScore);
    }
}
