using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SceneSetup
{
    // Cached TMP font
    private static TMP_FontAsset tmpFont;

    private static TMP_FontAsset GetTMPFont()
    {
        if (tmpFont != null) return tmpFont;

        // Try TMP default font
        tmpFont = TMP_Settings.defaultFontAsset;
        if (tmpFont != null) return tmpFont;

        // Search for any TMP font in project
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }

        if (tmpFont == null)
            Debug.LogWarning("SceneSetup: No TMP font found! Import TMP Essentials first (Window > TextMeshPro > Import TMP Essential Resources)");

        return tmpFont;
    }

    // ──────────────────────────────────────────────
    //  MAIN MENU
    // ──────────────────────────────────────────────
    [MenuItem("MiniGames/Setup MainMenu Scene")]
    public static void SetupMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ──
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
        cam.orthographic = false;
        camGO.transform.position = new Vector3(0, 2, -5);
        camGO.transform.rotation = Quaternion.Euler(15, 0, 0);
        camGO.tag = "MainCamera";
        camGO.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

        // ── Light ──
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // ── GameManager (persistent) ──
        var gmGO = new GameObject("[GameManager]");
        gmGO.AddComponent<GameManager>();

        // ── SaveManager (persistent) ──
        var saveGO = new GameObject("[SaveManager]");
        saveGO.AddComponent<SaveManager>();

        // ── SceneLoader (persistent) ──
        var slGO = new GameObject("[SceneLoader]");
        var sceneLoader = slGO.AddComponent<SceneLoader>();

        // Fade canvas
        var fadeCanvasGO = new GameObject("FadeCanvas");
        fadeCanvasGO.transform.SetParent(slGO.transform);
        var fadeCanvas = fadeCanvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;
        var fadeScaler = fadeCanvasGO.AddComponent<CanvasScaler>();
        fadeScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        fadeScaler.referenceResolution = new Vector2(1080, 1920);

        var fadeImgGO = new GameObject("FadeImage");
        fadeImgGO.transform.SetParent(fadeCanvasGO.transform, false);
        var fadeImg = fadeImgGO.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeImg.raycastTarget = false;
        var fadeRect = fadeImgGO.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;

        var fadeGroup = fadeImgGO.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;

        var slSO = new SerializedObject(sceneLoader);
        slSO.FindProperty("fadeCanvasGroup").objectReferenceValue = fadeGroup;
        slSO.ApplyModifiedPropertiesWithoutUndo();

        // ── Main UI Canvas ──
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Title ──
        var titleGO = CreateTMPText("Title", canvasGO.transform, "MINI GAMES", 72);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -80);
        titleRect.sizeDelta = new Vector2(800, 120);

        // ── Carousel ScrollRect ──
        var scrollGO = new GameObject("CarouselScroll");
        scrollGO.transform.SetParent(canvasGO.transform, false);
        var scrollRectTransform = scrollGO.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0.15f);
        scrollRectTransform.anchorMax = new Vector2(1, 0.85f);
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        // Viewport (child with Mask — this is what clips the content)
        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        var viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0);
        viewportImg.raycastTarget = true;
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;

        // Content (child of Viewport — holds the cards)
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        var contentRect = contentGO.AddComponent<RectTransform>();
        // Stretch full height, grow horizontally from left
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(0, 1);
        contentRect.pivot = new Vector2(0, 0.5f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        var hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 40;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.padding = new RectOffset(240, 240, 30, 30);
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // ScrollRect component (on the parent, references viewport + content)
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.decelerationRate = 0.05f;
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        // ── Pagination Dots ──
        var dotsGO = new GameObject("Dots");
        dotsGO.transform.SetParent(canvasGO.transform, false);
        var dotsRect = dotsGO.GetComponent<RectTransform>();
        if (dotsRect == null) dotsRect = dotsGO.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.5f, 0.1f);
        dotsRect.anchorMax = new Vector2(0.5f, 0.1f);
        dotsRect.sizeDelta = new Vector2(300, 30);
        var dotsHlg = dotsGO.AddComponent<HorizontalLayoutGroup>();
        dotsHlg.spacing = 15;
        dotsHlg.childAlignment = TextAnchor.MiddleCenter;
        dotsHlg.childForceExpandWidth = false;
        dotsHlg.childForceExpandHeight = false;

        // ── Dot Prefab ──
        EnsureFolder("Assets/_Project/Prefabs/UI");
        var dotPrefabGO = new GameObject("Dot");
        var dotImg = dotPrefabGO.AddComponent<Image>();
        dotImg.color = Color.white;
        var dotRectT = dotPrefabGO.GetComponent<RectTransform>();
        dotRectT.sizeDelta = new Vector2(20, 20);
        var dotLE = dotPrefabGO.AddComponent<LayoutElement>();
        dotLE.preferredWidth = 20;
        dotLE.preferredHeight = 20;
        string dotPrefabPath = "Assets/_Project/Prefabs/UI/Dot.prefab";
        PrefabUtility.SaveAsPrefabAsset(dotPrefabGO, dotPrefabPath);
        Object.DestroyImmediate(dotPrefabGO);
        var dotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dotPrefabPath);

        // ── MiniGameCard Prefab ──
        var cardPrefabGO = CreateMiniGameCardPrefab();
        string cardPrefabPath = "Assets/_Project/Prefabs/UI/MiniGameCard.prefab";
        PrefabUtility.SaveAsPrefabAsset(cardPrefabGO, cardPrefabPath);
        Object.DestroyImmediate(cardPrefabGO);
        var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPrefabPath);

        // ── CarouselController ──
        var carousel = scrollGO.AddComponent<CarouselController>();
        var carouselSO = new SerializedObject(carousel);
        carouselSO.FindProperty("scrollRect").objectReferenceValue = scrollRect;
        carouselSO.FindProperty("contentPanel").objectReferenceValue = contentRect;
        carouselSO.FindProperty("miniGameCardPrefab").objectReferenceValue = cardPrefab;
        carouselSO.FindProperty("dotsContainer").objectReferenceValue = dotsGO.transform;
        carouselSO.FindProperty("dotPrefab").objectReferenceValue = dotPrefab;
        carouselSO.ApplyModifiedPropertiesWithoutUndo();

        // ── EventSystem ──
        CreateEventSystem();

        // ── MiniGameData SO ──
        string soPath = "Assets/_Project/ScriptableObjects/StackTower_Data.asset";
        EnsureFolder("Assets/_Project/ScriptableObjects");
        // Always recreate to update fields
        var soData = AssetDatabase.LoadAssetAtPath<MiniGameData>(soPath);
        if (soData == null)
        {
            soData = ScriptableObject.CreateInstance<MiniGameData>();
            soData.gameName = "Stack Tower";
            soData.sceneName = "StackTower";
            soData.themeColor = new Color(0.2f, 0.7f, 0.9f);
            soData.description = "Empile les blocs le plus haut possible !";
            AssetDatabase.CreateAsset(soData, soPath);
        }

        // Wire GameManager
        var gmSO = new SerializedObject(gmGO.GetComponent<GameManager>());
        var list = gmSO.FindProperty("miniGameList");
        list.arraySize = 1;
        list.GetArrayElementAtIndex(0).objectReferenceValue = soData;
        gmSO.ApplyModifiedPropertiesWithoutUndo();

        // Save scene
        string scenePath = "Assets/_Project/Scenes/MainMenu.unity";
        EnsureFolder("Assets/_Project/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("<color=green>✓ MainMenu scene setup complete!</color>");
    }

    // ──────────────────────────────────────────────
    //  STACK TOWER
    // ──────────────────────────────────────────────
    [MenuItem("MiniGames/Setup StackTower Scene")]
    public static void SetupStackTower()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ──
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.14f);
        camGO.transform.position = new Vector3(3f, 5f, -3f);
        camGO.transform.rotation = Quaternion.Euler(35, -45, 0);
        cam.fieldOfView = 50;
        camGO.tag = "MainCamera";
        camGO.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

        // ── Lights ──
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.9f);
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        var fillLightGO = new GameObject("Fill Light");
        var fillLight = fillLightGO.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.6f, 0.7f, 1f);
        fillLight.intensity = 0.4f;
        fillLightGO.transform.rotation = Quaternion.Euler(-20, 160, 0);

        // ── StackTower Manager ──
        var towerGO = new GameObject("StackTowerManager");
        var towerManager = towerGO.AddComponent<StackTowerManager>();

        // ── UI Canvas ──
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Score ──
        var scoreGO = CreateTMPText("ScoreText", canvasGO.transform, "0", 96);
        var scoreRect = scoreGO.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.anchoredPosition = new Vector2(0, -60);
        scoreRect.sizeDelta = new Vector2(400, 120);

        // ── Perfect Text ──
        var perfectGO = CreateTMPText("PerfectText", canvasGO.transform, "PERFECT!", 56);
        var perfectRect = perfectGO.GetComponent<RectTransform>();
        perfectRect.anchorMin = new Vector2(0.5f, 0.65f);
        perfectRect.anchorMax = new Vector2(0.5f, 0.65f);
        perfectRect.sizeDelta = new Vector2(500, 80);
        var perfectTMP = perfectGO.GetComponent<TextMeshProUGUI>();
        perfectTMP.color = new Color(1f, 0.9f, 0.2f);
        perfectGO.SetActive(false);

        // ── Game Over Panel ──
        var gameOverGO = new GameObject("GameOverPanel");
        gameOverGO.transform.SetParent(canvasGO.transform, false);
        var goRect = gameOverGO.AddComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero;
        goRect.anchorMax = Vector2.one;
        goRect.offsetMin = Vector2.zero;
        goRect.offsetMax = Vector2.zero;
        var dimImg = gameOverGO.AddComponent<Image>();
        dimImg.color = new Color(0, 0, 0, 0.6f);

        // Container
        var containerGO = new GameObject("Container");
        containerGO.transform.SetParent(gameOverGO.transform, false);
        var containerRect = containerGO.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.3f);
        containerRect.anchorMax = new Vector2(0.9f, 0.7f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        var containerImg = containerGO.AddComponent<Image>();
        containerImg.color = new Color(0.15f, 0.15f, 0.22f, 0.95f);
        var vlg = containerGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(40, 40, 40, 40);

        var goTitleGO = CreateTMPText("GameOverTitle", containerGO.transform, "GAME OVER", 52);
        goTitleGO.AddComponent<LayoutElement>().preferredHeight = 70;

        var finalScoreGO = CreateTMPText("FinalScore", containerGO.transform, "Score: 0", 40);
        finalScoreGO.AddComponent<LayoutElement>().preferredHeight = 55;

        var highScoreGO = CreateTMPText("HighScore", containerGO.transform, "Best: 0", 32);
        highScoreGO.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);
        highScoreGO.AddComponent<LayoutElement>().preferredHeight = 45;

        // Buttons
        var btnRowGO = new GameObject("Buttons");
        btnRowGO.transform.SetParent(containerGO.transform, false);
        btnRowGO.AddComponent<RectTransform>();
        var btnRowHlg = btnRowGO.AddComponent<HorizontalLayoutGroup>();
        btnRowHlg.spacing = 30;
        btnRowHlg.childAlignment = TextAnchor.MiddleCenter;
        btnRowHlg.childForceExpandWidth = false;
        btnRowHlg.childForceExpandHeight = false;
        btnRowGO.AddComponent<LayoutElement>().preferredHeight = 80;

        var restartBtn = CreateButton("RestartButton", btnRowGO.transform, "REJOUER",
            new Color(0.2f, 0.75f, 0.4f), 300, 70);
        var menuBtn = CreateButton("MenuButton", btnRowGO.transform, "MENU",
            new Color(0.5f, 0.5f, 0.6f), 220, 70);

        gameOverGO.SetActive(false);

        // ── Wire StackTowerUI ──
        var towerUI = canvasGO.AddComponent<StackTowerUI>();
        var uiSO = new SerializedObject(towerUI);
        uiSO.FindProperty("scoreText").objectReferenceValue = scoreGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("perfectText").objectReferenceValue = perfectTMP;
        uiSO.FindProperty("gameOverPanel").objectReferenceValue = gameOverGO;
        uiSO.FindProperty("finalScoreText").objectReferenceValue = finalScoreGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("highScoreText").objectReferenceValue = highScoreGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.FindProperty("towerManager").objectReferenceValue = towerManager;
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        // ── EventSystem ──
        CreateEventSystem();

        // Save scene
        string scenePath = "Assets/_Project/Scenes/StackTower.unity";
        EnsureFolder("Assets/_Project/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("<color=green>✓ StackTower scene setup complete!</color>");
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

    private static GameObject CreateMiniGameCardPrefab()
    {
        var cardGO = new GameObject("MiniGameCard");
        var cardRect = cardGO.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(600, 900);

        var cardLE = cardGO.AddComponent<LayoutElement>();
        cardLE.preferredWidth = 600;
        cardLE.preferredHeight = 900;
        cardLE.minWidth = 600;
        cardLE.minHeight = 900;

        // Background
        var bgImg = cardGO.AddComponent<Image>();
        bgImg.color = new Color(0.18f, 0.18f, 0.25f, 0.95f);
        bgImg.raycastTarget = true;

        var vlg = cardGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(40, 40, 50, 40);

        // Icon placeholder
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(cardGO.transform, false);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.3f, 0.3f, 0.4f);
        iconImg.raycastTarget = false;
        var iconLE = iconGO.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 250;
        iconLE.preferredHeight = 250;
        iconLE.minHeight = 250;

        // Title
        var titleGO = CreateTMPText("Title", cardGO.transform, "Game Name", 48);
        titleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 70;
        titleLE.minHeight = 70;

        // Description
        var descGO = CreateTMPText("Description", cardGO.transform, "Description du jeu", 26);
        var descTMP = descGO.GetComponent<TextMeshProUGUI>();
        descTMP.color = new Color(0.7f, 0.7f, 0.75f);
        descTMP.textWrappingMode = TextWrappingModes.Normal;
        var descLE = descGO.AddComponent<LayoutElement>();
        descLE.preferredHeight = 100;
        descLE.minHeight = 60;

        // High Score
        var hsGO = CreateTMPText("HighScore", cardGO.transform, "Best: 0", 34);
        var hsTMP = hsGO.GetComponent<TextMeshProUGUI>();
        hsTMP.color = new Color(1f, 0.85f, 0.3f);
        var hsLE = hsGO.AddComponent<LayoutElement>();
        hsLE.preferredHeight = 50;
        hsLE.minHeight = 50;

        // Spacer
        var spacer = new GameObject("Spacer");
        spacer.transform.SetParent(cardGO.transform, false);
        spacer.AddComponent<RectTransform>();
        var spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.flexibleHeight = 1;

        // Play button
        var playBtn = CreateButton("PlayButton", cardGO.transform, "JOUER",
            new Color(0.2f, 0.75f, 0.4f), 350, 90);
        var playBtnLE = playBtn.AddComponent<LayoutElement>();
        playBtnLE.preferredWidth = 350;
        playBtnLE.preferredHeight = 90;
        playBtnLE.minHeight = 90;

        // MiniGameCard component
        var card = cardGO.AddComponent<MiniGameCard>();
        var cardSO = new SerializedObject(card);
        cardSO.FindProperty("iconImage").objectReferenceValue = iconImg;
        cardSO.FindProperty("titleText").objectReferenceValue = titleGO.GetComponent<TextMeshProUGUI>();
        cardSO.FindProperty("highScoreText").objectReferenceValue = hsTMP;
        cardSO.FindProperty("descriptionText").objectReferenceValue = descTMP;
        cardSO.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();
        cardSO.FindProperty("backgroundImage").objectReferenceValue = bgImg;
        cardSO.ApplyModifiedPropertiesWithoutUndo();

        return cardGO;
    }

    private static GameObject CreateTMPText(string name, Transform parent, string text, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        // Assign TMP font
        var font = GetTMPFont();
        if (font != null)
            tmp.font = font;

        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string label, Color color, float w, float h)
    {
        var btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(w, h);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = color;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // Button color tint
        var colors = btn.colors;
        colors.highlightedColor = new Color(1, 1, 1, 0.9f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        var textGO = CreateTMPText("Text", btnGO.transform, label, 30);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        return btnGO;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            return;

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
