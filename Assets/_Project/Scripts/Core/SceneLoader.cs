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
