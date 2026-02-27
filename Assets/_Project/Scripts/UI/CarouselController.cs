using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarouselController : MonoBehaviour, IEndDragHandler
{
    [Header("Layout")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private GameObject miniGameCardPrefab;

    [Header("Snap")]
    [SerializeField] private float snapSpeed = 10f;
    [SerializeField] private float snapThreshold = 0.01f;

    [Header("Pagination")]
    [SerializeField] private Transform dotsContainer;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Color dotActive = Color.white;
    [SerializeField] private Color dotInactive = new Color(1f, 1f, 1f, 0.4f);

    private List<MiniGameCard> cards = new List<MiniGameCard>();
    private List<Image> dots = new List<Image>();
    private int currentIndex;
    private bool isSnapping;
    private float targetNormalized;
    private int totalCards;
    private bool isDragging;

    private void Start()
    {
        PopulateCards();
    }

    private void PopulateCards()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("CarouselController: GameManager.Instance is null");
            return;
        }

        List<MiniGameData> games = GameManager.Instance.miniGameList;
        totalCards = games.Count;
        if (totalCards == 0)
        {
            Debug.LogWarning("CarouselController: No mini games in list");
            return;
        }

        // Clear existing
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);
        if (dotsContainer != null)
        {
            foreach (Transform child in dotsContainer)
                Destroy(child.gameObject);
        }
        cards.Clear();
        dots.Clear();

        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardObj = Instantiate(miniGameCardPrefab, contentPanel);
            MiniGameCard card = cardObj.GetComponent<MiniGameCard>();
            card.Setup(games[i]);
            cards.Add(card);

            // Dot
            if (dotPrefab != null && dotsContainer != null)
            {
                GameObject dot = Instantiate(dotPrefab, dotsContainer);
                Image dotImg = dot.GetComponent<Image>();
                dots.Add(dotImg);
            }
        }

        currentIndex = 0;
        UpdateDots();

        // Wait one frame for layout to compute, then snap
        StartCoroutine(InitialSnap());
    }

    private IEnumerator InitialSnap()
    {
        yield return null; // wait for layout rebuild
        yield return null; // extra frame for safety

        // Force the first card to be centered and interactable
        scrollRect.horizontalNormalizedPosition = 0f;
        targetNormalized = 0f;
        currentIndex = 0;

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetCentered(i == 0, force: true);
            cards[i].transform.localScale = Vector3.one * (i == 0 ? 1f : 0.85f);
        }
        UpdateDots();
    }

    private void Update()
    {
        if (isSnapping && !isDragging)
        {
            float current = scrollRect.horizontalNormalizedPosition;
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(current, targetNormalized, Time.deltaTime * snapSpeed);

            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetNormalized) < snapThreshold)
            {
                scrollRect.horizontalNormalizedPosition = targetNormalized;
                isSnapping = false;
            }

            UpdateCardScales();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        SnapToNearest();
    }

    public void OnBeginDrag()
    {
        isDragging = true;
        isSnapping = false;
    }

    private void SnapToNearest()
    {
        if (totalCards <= 1)
        {
            targetNormalized = 0f;
            isSnapping = true;
            currentIndex = 0;
            UpdateDots();
            UpdateCardScales();
            return;
        }

        float normalizedPos = scrollRect.horizontalNormalizedPosition;
        float step = 1f / (totalCards - 1);
        int nearest = Mathf.RoundToInt(normalizedPos / step);
        nearest = Mathf.Clamp(nearest, 0, totalCards - 1);

        targetNormalized = nearest * step;
        currentIndex = nearest;
        isSnapping = true;
        UpdateDots();
        UpdateCardScales();
    }

    private void UpdateDots()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].color = (i == currentIndex) ? dotActive : dotInactive;
        }
    }

    private void UpdateCardScales()
    {
        if (totalCards == 0) return;

        float normalizedPos = scrollRect.horizontalNormalizedPosition;
        float step = totalCards > 1 ? 1f / (totalCards - 1) : 1f;

        for (int i = 0; i < cards.Count; i++)
        {
            float cardNorm = totalCards > 1 ? i * step : 0f;
            float distance = Mathf.Abs(normalizedPos - cardNorm);
            float scale = Mathf.Lerp(1f, 0.85f, Mathf.Clamp01(distance * Mathf.Max(totalCards - 1, 1)));
            cards[i].transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.85f, 1f);
            cards[i].SetCentered(i == currentIndex);
        }
    }
}
