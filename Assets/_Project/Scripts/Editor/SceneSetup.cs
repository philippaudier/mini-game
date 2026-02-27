using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneSetup
{
    private const string UxmlRoot = "Assets/_Project/UI/UXML/";
    private const string PanelSettingsPath = "Assets/_Project/UI/DefaultPanelSettings.asset";

    // ──────────────────────────────────────────────
    //  PANEL SETTINGS
    // ──────────────────────────────────────────────
    private static PanelSettings GetOrCreatePanelSettings()
    {
        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
        if (ps != null) return ps;

        ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1080, 1920);
        ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = 0.5f;

        EnsureFolder("Assets/_Project/UI");
        AssetDatabase.CreateAsset(ps, PanelSettingsPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created PanelSettings at {PanelSettingsPath}");
        return ps;
    }

    // ──────────────────────────────────────────────
    //  MAIN MENU
    // ──────────────────────────────────────────────
    [MenuItem("MiniGames/Setup MainMenu Scene")]
    public static void SetupMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var panelSettings = GetOrCreatePanelSettings();

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

        // ── SceneLoader (persistent) with FadeOverlay ──
        var slGO = new GameObject("[SceneLoader]");
        var sceneLoader = slGO.AddComponent<SceneLoader>();

        var fadeGO = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(slGO.transform);
        var fadeDoc = fadeGO.AddComponent<UIDocument>();
        fadeDoc.panelSettings = panelSettings;
        fadeDoc.sortingOrder = 100;
        var fadeUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlRoot + "FadeOverlay.uxml");
        fadeDoc.visualTreeAsset = fadeUxml;

        // Wire SceneLoader → FadeUIDocument
        var slSO = new SerializedObject(sceneLoader);
        slSO.FindProperty("fadeUIDocument").objectReferenceValue = fadeDoc;
        slSO.ApplyModifiedPropertiesWithoutUndo();

        // ── Main Menu UI ──
        var menuGO = new GameObject("MainMenuUI");
        var menuDoc = menuGO.AddComponent<UIDocument>();
        menuDoc.panelSettings = panelSettings;
        menuDoc.sortingOrder = 0;
        var menuUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlRoot + "MainMenu.uxml");
        menuDoc.visualTreeAsset = menuUxml;

        var menuCtrl = menuGO.AddComponent<MainMenuController>();
        var buttonTemplateAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlRoot + "MainMenuButton.uxml");
        var menuCtrlSO = new SerializedObject(menuCtrl);
        menuCtrlSO.FindProperty("buttonTemplate").objectReferenceValue = buttonTemplateAsset;
        menuCtrlSO.FindProperty("uiDocument").objectReferenceValue = menuDoc;
        menuCtrlSO.ApplyModifiedPropertiesWithoutUndo();

        // ── EventSystem ──
        CreateEventSystem();

        // ── MiniGameData SO ──
        string soPath = "Assets/_Project/ScriptableObjects/StackTower_Data.asset";
        EnsureFolder("Assets/_Project/ScriptableObjects");
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

        Debug.Log("<color=green>✓ MainMenu scene setup complete! (UI Toolkit)</color>");
    }

    // ──────────────────────────────────────────────
    //  STACK TOWER
    // ──────────────────────────────────────────────
    [MenuItem("MiniGames/Setup StackTower Scene")]
    public static void SetupStackTower()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var panelSettings = GetOrCreatePanelSettings();

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

        // ── HUD UI (UIDocument) ──
        var hudGO = new GameObject("StackTowerHUD");
        var hudDoc = hudGO.AddComponent<UIDocument>();
        hudDoc.panelSettings = panelSettings;
        hudDoc.sortingOrder = 0;
        var hudUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlRoot + "StackTowerHUD.uxml");
        hudDoc.visualTreeAsset = hudUxml;

        var hudCtrl = hudGO.AddComponent<StackTowerUIController>();
        var hudSO = new SerializedObject(hudCtrl);
        hudSO.FindProperty("uiDocument").objectReferenceValue = hudDoc;
        hudSO.FindProperty("towerManager").objectReferenceValue = towerManager;
        hudSO.ApplyModifiedPropertiesWithoutUndo();

        // ── EventSystem ──
        CreateEventSystem();

        // Save scene
        string scenePath = "Assets/_Project/Scenes/StackTower.unity";
        EnsureFolder("Assets/_Project/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("<color=green>✓ StackTower scene setup complete! (UI Toolkit)</color>");
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

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
