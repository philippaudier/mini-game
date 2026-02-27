using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private UIDocument fadeUIDocument;
    [SerializeField] private float fadeDuration = 0.4f;

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureFadeOverlay();
    }

    private void EnsureFadeOverlay()
    {
        if (fadeUIDocument != null) return;

        // Load UXML asset
        var fadeUxml = Resources.Load<VisualTreeAsset>("FadeOverlay");
        // Try from Assets path via runtime loading
        if (fadeUxml == null)
        {
            // Create fade overlay purely in code — no UXML dependency needed at runtime
            var fadeGO = new GameObject("FadeOverlay");
            fadeGO.transform.SetParent(transform);
            fadeUIDocument = fadeGO.AddComponent<UIDocument>();
            fadeUIDocument.sortingOrder = 100;

            // We need to wait one frame for the UIDocument to initialize,
            // then build the overlay manually
            StartCoroutine(BuildFadeOverlayNextFrame());
            return;
        }

        var go = new GameObject("FadeOverlay");
        go.transform.SetParent(transform);
        fadeUIDocument = go.AddComponent<UIDocument>();
        fadeUIDocument.sortingOrder = 100;
        fadeUIDocument.visualTreeAsset = fadeUxml;
    }

    private IEnumerator BuildFadeOverlayNextFrame()
    {
        yield return null; // wait for UIDocument to initialize

        var root = fadeUIDocument.rootVisualElement;
        if (root == null) yield break;

        var overlay = new VisualElement();
        overlay.name = "fade-overlay";
        overlay.pickingMode = PickingMode.Ignore;
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = Color.black;
        overlay.style.opacity = 0f;
        root.Add(overlay);
    }

    private VisualElement GetFadeOverlay()
    {
        if (fadeUIDocument == null) return null;
        return fadeUIDocument.rootVisualElement?.Q("fade-overlay");
    }

    public void SetFadeUIDocument(UIDocument doc)
    {
        fadeUIDocument = doc;
    }

    public void LoadScene(string sceneName)
    {
        if (!isLoading)
            StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // Fade out (to black)
        var overlay = GetFadeOverlay();
        if (overlay != null)
        {
            yield return StartCoroutine(Fade(overlay, 0f, 1f));
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        // Re-query after scene load (visual tree may have been rebuilt)
        overlay = GetFadeOverlay();
        if (overlay != null)
        {
            yield return StartCoroutine(Fade(overlay, 1f, 0f));
        }

        isLoading = false;
    }

    private IEnumerator Fade(VisualElement overlay, float from, float to)
    {
        float elapsed = 0f;
        overlay.style.opacity = from;
        overlay.pickingMode = PickingMode.Position; // Block input during fade

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            overlay.style.opacity = Mathf.Lerp(from, to, t);
            yield return null;
        }

        overlay.style.opacity = to;
        overlay.pickingMode = to > 0.5f ? PickingMode.Position : PickingMode.Ignore;
    }
}
