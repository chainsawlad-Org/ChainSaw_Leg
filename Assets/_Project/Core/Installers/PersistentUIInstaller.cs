using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PersistentUIInstaller : MonoInstaller
{
    [SerializeField] private Transform uiRoot;

    public override void InstallBindings()
    {
        PauseMenuView pauseMenuView = BuildPauseMenuView(uiRoot, out SaveBrowserView saveBrowserView);

        Container.Bind<PauseMenuView>()
            .FromInstance(pauseMenuView)
            .AsSingle();

        Container.Bind<SaveBrowserView>()
            .FromInstance(saveBrowserView)
            .AsSingle();

        Container.BindInterfacesTo<PauseMenuPresenter>()
            .AsSingle()
            .NonLazy();
    }

    private static PauseMenuView BuildPauseMenuView(
        Transform parent,
        out SaveBrowserView saveBrowserView)
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
        SetPanelRect(saveBrowserPanel, new Vector2(0.5f, 0.5f), new Vector2(820f, 540f));

        var saveBrowserTitle = CreateLabel("Title", saveBrowserPanel, "Сохранения", 32, TextAnchor.MiddleCenter);
        SetPanelRect(saveBrowserTitle, new Vector2(0.5f, 0.91f), new Vector2(320f, 44f));

        var rowsObject = new GameObject("Rows", typeof(RectTransform));
        rowsObject.transform.SetParent(saveBrowserPanel, false);
        RectTransform rowsContainer = rowsObject.GetComponent<RectTransform>();
        SetTopRect(rowsContainer, new Vector2(0.5f, 0.81f), new Vector2(760f, 230f));

        SaveSlotView rowTemplate = CreateSaveSlotTemplate(rowsContainer);

        RectTransform statusRect = CreateLabel("Status", saveBrowserPanel, string.Empty, 24, TextAnchor.MiddleCenter);
        SetPanelRect(statusRect, new Vector2(0.5f, 0.52f), new Vector2(500f, 50f));
        Text statusText = statusRect.GetComponent<Text>();

        RectTransform errorRect = CreateLabel("Error", saveBrowserPanel, string.Empty, 20, TextAnchor.MiddleCenter);
        SetPanelRect(errorRect, new Vector2(0.5f, 0.2f), new Vector2(680f, 44f));
        Text errorText = errorRect.GetComponent<Text>();
        errorText.color = new Color(1f, 0.35f, 0.3f, 1f);
        errorText.gameObject.SetActive(false);

        Button backButton = CreateButton("BackButton", saveBrowserPanel, "Назад");
        SetPanelRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.09f), new Vector2(220f, 48f));

        saveBrowserView = saveBrowserPanel.gameObject.AddComponent<SaveBrowserView>();
        saveBrowserView.SetReferences(rowsContainer, rowTemplate, statusText, errorText);

        var view = canvasObject.AddComponent<PauseMenuView>();
        view.SetReferences(
            rootPanel.gameObject,
            pausePanel.gameObject,
            saveBrowserPanel.gameObject,
            continueButton,
            savesButton,
            exitButton,
            backButton,
            saveBrowserView);
        view.ShowRoot(false);
        canvasObject.SetActive(true);

        return view;
    }

    private static SaveSlotView CreateSaveSlotTemplate(Transform parent)
    {
        var rowObject = new GameObject("SaveSlotTemplate", typeof(RectTransform), typeof(Image));
        rowObject.transform.SetParent(parent, false);

        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        SetTopRect(rowRect, new Vector2(0.5f, 1f), new Vector2(740f, 64f));
        rowObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Text kindText = CreateRowText("Kind", rowRect, new Vector2(-305f, 0f), new Vector2(110f, 54f));
        Text locationText = CreateRowText("Location", rowRect, new Vector2(-165f, 0f), new Vector2(170f, 54f));
        Text timestampText = CreateRowText("Timestamp", rowRect, new Vector2(20f, 0f), new Vector2(190f, 54f));
        Text sceneText = CreateRowText("Scene", rowRect, new Vector2(175f, 0f), new Vector2(110f, 54f));

        Button loadButton = CreateButton("LoadButton", rowRect, "Загрузить");
        SetLocalRect(loadButton.GetComponent<RectTransform>(), new Vector2(305f, 0f), new Vector2(120f, 44f));
        loadButton.GetComponentInChildren<Text>().fontSize = 18;

        SaveSlotView rowView = rowObject.AddComponent<SaveSlotView>();
        rowView.SetReferences(kindText, locationText, timestampText, sceneText, loadButton);
        rowObject.SetActive(false);
        return rowView;
    }

    private static Text CreateRowText(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size)
    {
        RectTransform labelRect = CreateLabel(name, parent, string.Empty, 18, TextAnchor.MiddleCenter);
        SetLocalRect(labelRect, position, size);
        return labelRect.GetComponent<Text>();
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

    private static void SetTopRect(RectTransform rectTransform, Vector2 anchor, Vector2 size)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }

    private static void SetLocalRect(RectTransform rectTransform, Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }
}
