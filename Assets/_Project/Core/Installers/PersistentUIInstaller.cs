using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PersistentUIInstaller : MonoInstaller
{
    [SerializeField] private Transform uiRoot;

    public override void InstallBindings()
    {
        PauseMenuView pauseMenuView = BuildPauseMenuView(uiRoot);

        Container.Bind<PauseMenuView>()
            .FromInstance(pauseMenuView)
            .AsSingle();

        Container.BindInterfacesTo<PauseMenuPresenter>()
            .AsSingle()
            .NonLazy();
    }

    private static PauseMenuView BuildPauseMenuView(Transform parent)
    {
        var canvasObject = new GameObject(
            "PauseMenuCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.transform.SetParent(parent, false);
        canvasObject.SetActive(false);

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        Stretch(canvasRect);

        var rootPanel = CreatePanel("Root", canvasRect, new Color(0f, 0f, 0f, 0.55f));
        var pausePanel = CreatePanel("PausePanel", rootPanel, new Color(0.15f, 0.15f, 0.15f, 0.95f));
        SetPanelRect(pausePanel, new Vector2(0.5f, 0.5f), new Vector2(420f, 320f));

        var title = CreateLabel("Title", pausePanel, "Pause", 36, TextAnchor.MiddleCenter);
        SetPanelRect(title, new Vector2(0.5f, 0.84f), new Vector2(260f, 50f));

        Button continueButton = CreateButton("ContinueButton", pausePanel, "Продолжить");
        SetPanelRect(continueButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.62f), new Vector2(280f, 48f));

        Button savesButton = CreateButton("SavesButton", pausePanel, "Сохранения");
        SetPanelRect(savesButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.45f), new Vector2(280f, 48f));

        Button exitButton = CreateButton("ExitButton", pausePanel, "Выйти в главное меню");
        SetPanelRect(exitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.28f), new Vector2(280f, 48f));

        var saveBrowserPanel = CreatePanel("SaveBrowserPanel", rootPanel, new Color(0.12f, 0.12f, 0.12f, 0.96f));
        SetPanelRect(saveBrowserPanel, new Vector2(0.5f, 0.5f), new Vector2(640f, 420f));

        var saveBrowserTitle = CreateLabel("Title", saveBrowserPanel, "Save Browser", 32, TextAnchor.MiddleCenter);
        SetPanelRect(saveBrowserTitle, new Vector2(0.5f, 0.88f), new Vector2(320f, 44f));

        var placeholder = CreateLabel("Placeholder", saveBrowserPanel, "Панель сохранений", 24, TextAnchor.MiddleCenter);
        SetPanelRect(placeholder, new Vector2(0.5f, 0.52f), new Vector2(360f, 44f));

        Button backButton = CreateButton("BackButton", saveBrowserPanel, "Назад");
        SetPanelRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.16f), new Vector2(220f, 48f));

        var view = canvasObject.AddComponent<PauseMenuView>();
        view.SetReferences(rootPanel.gameObject, pausePanel.gameObject, saveBrowserPanel.gameObject, continueButton, savesButton, exitButton, backButton);
        view.ShowRoot(false);
        canvasObject.SetActive(true);

        return view;
    }

    private static RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        Stretch(rectTransform);

        Image image = panelObject.GetComponent<Image>();
        image.color = color;

        return rectTransform;
    }

    private static Button CreateButton(string name, Transform parent, string label)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        Button button = buttonObject.GetComponent<Button>();

        var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        Stretch(textRect);

        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = 24;
        text.font = GetBuiltInFont();

        return button;
    }

    private static RectTransform CreateLabel(string name, Transform parent, string value, int fontSize, TextAnchor anchor)
    {
        var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(parent, false);

        Text text = labelObject.GetComponent<Text>();
        text.text = value;
        text.alignment = anchor;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.font = GetBuiltInFont();

        return labelObject.GetComponent<RectTransform>();
    }

    private static Font GetBuiltInFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private static void SetPanelRect(RectTransform rectTransform, Vector2 anchor, Vector2 size)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }
}
