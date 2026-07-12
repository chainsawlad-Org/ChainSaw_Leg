using ChainSawLeg.Core.SaveSystem;
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

        CheckpointSaveMenuView checkpointSaveMenuView = BuildCheckpointSaveMenuView(uiRoot);

        Container.Bind<CheckpointSaveMenuView>()
            .FromInstance(checkpointSaveMenuView)
            .AsSingle();

        Container.BindInterfacesTo<CheckpointSaveMenuPresenter>()
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
        SetPanelRect(saveBrowserPanel, new Vector2(0.5f, 0.5f), new Vector2(960f, 600f));

        var saveBrowserTitle = CreateLabel("Title", saveBrowserPanel, "Сохранения", 32, TextAnchor.MiddleCenter);
        SetTopRect(saveBrowserTitle, new Vector2(0.5f, 1f), new Vector2(320f, 44f));
        saveBrowserTitle.anchoredPosition = new Vector2(0f, -22f);

        CreateColumnHeader("KindHeader", saveBrowserPanel, "Тип", new Vector2(-350f, -92f), new Vector2(120f, 28f));
        CreateColumnHeader("LocationHeader", saveBrowserPanel, "Место", new Vector2(-205f, -92f), new Vector2(170f, 28f));
        CreateColumnHeader("TimestampHeader", saveBrowserPanel, "Дата", new Vector2(0f, -92f), new Vector2(200f, 28f));
        CreateColumnHeader("SceneHeader", saveBrowserPanel, "Сцена", new Vector2(175f, -92f), new Vector2(120f, 28f));

        var rowsScrollViewObject = new GameObject("RowsScrollView", typeof(RectTransform), typeof(ScrollRect));
        rowsScrollViewObject.transform.SetParent(saveBrowserPanel, false);
        RectTransform rowsScrollViewRect = rowsScrollViewObject.GetComponent<RectTransform>();
        rowsScrollViewRect.anchorMin = new Vector2(0.5f, 1f);
        rowsScrollViewRect.anchorMax = new Vector2(0.5f, 1f);
        rowsScrollViewRect.pivot = new Vector2(0.5f, 1f);
        rowsScrollViewRect.anchoredPosition = new Vector2(-20f, -116f);
        rowsScrollViewRect.sizeDelta = new Vector2(860f, 350f);

        var rowsViewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        rowsViewportObject.transform.SetParent(rowsScrollViewObject.transform, false);
        RectTransform rowsViewportRect = rowsViewportObject.GetComponent<RectTransform>();
        Stretch(rowsViewportRect);
        Image rowsViewportImage = rowsViewportObject.GetComponent<Image>();
        rowsViewportImage.color = Color.clear;
        rowsViewportImage.raycastTarget = true;

        var rowsObject = new GameObject("Rows", typeof(RectTransform));
        rowsObject.transform.SetParent(rowsViewportObject.transform, false);
        RectTransform rowsContainer = rowsObject.GetComponent<RectTransform>();
        rowsContainer.anchorMin = new Vector2(0f, 1f);
        rowsContainer.anchorMax = new Vector2(1f, 1f);
        rowsContainer.pivot = new Vector2(0.5f, 1f);
        rowsContainer.anchoredPosition = Vector2.zero;
        rowsContainer.sizeDelta = new Vector2(0f, 350f);

        SaveSlotView rowTemplate = CreateSaveSlotTemplate(rowsContainer);

        Scrollbar rowsScrollbar = CreateVerticalScrollbar(
            saveBrowserPanel,
            new Vector2(0.5f, 1f),
            new Vector2(430f, -116f),
            350f);

        ScrollRect rowsScrollRect = rowsScrollViewObject.GetComponent<ScrollRect>();
        rowsScrollRect.viewport = rowsViewportRect;
        rowsScrollRect.content = rowsContainer;
        rowsScrollRect.horizontal = false;
        rowsScrollRect.vertical = true;
        rowsScrollRect.movementType = ScrollRect.MovementType.Clamped;
        rowsScrollRect.scrollSensitivity = 25f;
        rowsScrollRect.verticalScrollbar = rowsScrollbar;
        rowsScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        RectTransform statusRect = CreateLabel("Status", saveBrowserPanel, string.Empty, 24, TextAnchor.MiddleCenter);
        SetPanelRect(statusRect, new Vector2(0.5f, 0.5f), new Vector2(500f, 50f));
        Text statusText = statusRect.GetComponent<Text>();

        RectTransform errorRect = CreateLabel("Error", saveBrowserPanel, string.Empty, 20, TextAnchor.MiddleCenter);
        SetPanelRect(errorRect, new Vector2(0.5f, 0.14f), new Vector2(680f, 44f));
        Text errorText = errorRect.GetComponent<Text>();
        errorText.color = new Color(1f, 0.35f, 0.3f, 1f);
        errorText.gameObject.SetActive(false);

        Button backButton = CreateButton("BackButton", saveBrowserPanel, "Назад");
        SetPanelRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.065f), new Vector2(220f, 48f));

        saveBrowserView = saveBrowserPanel.gameObject.AddComponent<SaveBrowserView>();
        saveBrowserView.SetReferences(rowsContainer, rowTemplate, statusText, errorText, rowsScrollRect);

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
        SetTopRect(rowRect, new Vector2(0.5f, 1f), new Vector2(840f, 56f));
        rowObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Text kindText = CreateRowText("Kind", rowRect, new Vector2(-350f, 0f), new Vector2(120f, 48f));
        Text locationText = CreateRowText("Location", rowRect, new Vector2(-205f, 0f), new Vector2(170f, 48f));
        Text timestampText = CreateRowText("Timestamp", rowRect, new Vector2(0f, 0f), new Vector2(200f, 48f));
        Text sceneText = CreateRowText("Scene", rowRect, new Vector2(175f, 0f), new Vector2(120f, 48f));

        Button loadButton = CreateButton("LoadButton", rowRect, "Загрузить");
        SetLocalRect(loadButton.GetComponent<RectTransform>(), new Vector2(350f, 0f), new Vector2(130f, 42f));
        loadButton.GetComponentInChildren<Text>().fontSize = 18;

        SaveSlotView rowView = rowObject.AddComponent<SaveSlotView>();
        rowView.SetReferences(kindText, locationText, timestampText, sceneText, loadButton);
        rowObject.SetActive(false);
        return rowView;
    }

    private static CheckpointSaveMenuView BuildCheckpointSaveMenuView(Transform parent)
    {
        var canvasObject = new GameObject(
            "CheckpointSaveMenuCanvas",
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
        var panel = CreatePanel("Panel", rootPanel, new Color(0.12f, 0.12f, 0.12f, 0.96f));
        SetPanelRect(panel, new Vector2(0.5f, 0.5f), new Vector2(960f, 600f));

        var title = CreateLabel("Title", panel, "Сохранение", 32, TextAnchor.MiddleCenter);
        SetTopRect(title, new Vector2(0.5f, 1f), new Vector2(320f, 44f));
        title.anchoredPosition = new Vector2(0f, -22f);

        CreateColumnHeader("SlotHeader", panel, "Слот", new Vector2(-350f, -92f), new Vector2(130f, 28f));
        CreateColumnHeader("PlaceHeader", panel, "Место", new Vector2(-125f, -92f), new Vector2(300f, 28f));
        CreateColumnHeader("DateHeader", panel, "Дата", new Vector2(145f, -92f), new Vector2(190f, 28f));

        var scrollViewObject = new GameObject("SlotsScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollViewObject.transform.SetParent(panel, false);
        RectTransform scrollViewRect = scrollViewObject.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0.5f, 1f);
        scrollViewRect.anchorMax = new Vector2(0.5f, 1f);
        scrollViewRect.pivot = new Vector2(0.5f, 1f);
        scrollViewRect.anchoredPosition = new Vector2(-20f, -116f);
        scrollViewRect.sizeDelta = new Vector2(860f, 350f);

        var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewportObject.transform.SetParent(scrollViewObject.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        Stretch(viewportRect);
        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = Color.clear;
        viewportImage.raycastTarget = true;

        var rowsObject = new GameObject("Rows", typeof(RectTransform));
        rowsObject.transform.SetParent(viewportObject.transform, false);
        RectTransform rowsContainer = rowsObject.GetComponent<RectTransform>();
        rowsContainer.anchorMin = new Vector2(0f, 1f);
        rowsContainer.anchorMax = new Vector2(1f, 1f);
        rowsContainer.pivot = new Vector2(0.5f, 1f);
        rowsContainer.anchoredPosition = Vector2.zero;
        rowsContainer.sizeDelta = new Vector2(0f, GameSaveSlotCatalog.CheckpointSlotIds.Count * 64f);

        var rows = new CheckpointSaveSlotView[GameSaveSlotCatalog.CheckpointSlotIds.Count];

        for (int index = 0; index < rows.Length; index++)
            rows[index] = CreateCheckpointSaveSlotRow(rowsContainer, index);

        Scrollbar scrollbar = CreateVerticalScrollbar(panel, new Vector2(0.5f, 1f), new Vector2(430f, -116f), 350f);

        ScrollRect scrollRect = scrollViewObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = rowsContainer;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 25f;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        Button backButton = CreateButton("BackButton", panel, "Назад");
        RectTransform backButtonRect = backButton.GetComponent<RectTransform>();
        backButtonRect.anchorMin = new Vector2(0.5f, 0f);
        backButtonRect.anchorMax = new Vector2(0.5f, 0f);
        backButtonRect.pivot = new Vector2(0.5f, 0f);
        backButtonRect.anchoredPosition = new Vector2(0f, 24f);
        backButtonRect.sizeDelta = new Vector2(220f, 48f);

        var view = panel.gameObject.AddComponent<CheckpointSaveMenuView>();
        view.SetReferences(rows, backButton, rootPanel.gameObject, scrollRect);
        canvasObject.SetActive(true);

        return view;
    }

    private static Scrollbar CreateVerticalScrollbar(
        Transform parent,
        Vector2 anchor,
        Vector2 anchoredPosition,
        float height)
    {
        var scrollbarObject = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        scrollbarObject.transform.SetParent(parent, false);

        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = anchor;
        scrollbarRect.anchorMax = anchor;
        scrollbarRect.pivot = new Vector2(0.5f, 1f);
        scrollbarRect.anchoredPosition = anchoredPosition;
        scrollbarRect.sizeDelta = new Vector2(20f, height);

        scrollbarObject.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 1f);

        var slidingAreaObject = new GameObject("SlidingArea", typeof(RectTransform));
        slidingAreaObject.transform.SetParent(scrollbarObject.transform, false);
        RectTransform slidingAreaRect = slidingAreaObject.GetComponent<RectTransform>();
        Stretch(slidingAreaRect);

        var handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObject.transform.SetParent(slidingAreaObject.transform, false);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = new Vector2(1f, 0.3f);
        handleRect.sizeDelta = Vector2.zero;
        handleRect.anchoredPosition = Vector2.zero;

        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(0.55f, 0.55f, 0.55f, 1f);

        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;

        return scrollbar;
    }

    private static CheckpointSaveSlotView CreateCheckpointSaveSlotRow(Transform parent, int index)
    {
        var rowObject = new GameObject($"SaveSlot_{index}", typeof(RectTransform), typeof(Image));
        rowObject.transform.SetParent(parent, false);

        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        SetTopRect(rowRect, new Vector2(0.5f, 1f), new Vector2(840f, 56f));
        rowRect.anchoredPosition = new Vector2(0f, -index * 64f);
        rowObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Text titleText = CreateRowText("Title", rowRect, new Vector2(-350f, 0f), new Vector2(130f, 48f));
        Text detailText = CreateRowText("Detail", rowRect, new Vector2(-125f, 0f), new Vector2(300f, 48f));
        Text timestampText = CreateRowText("Timestamp", rowRect, new Vector2(145f, 0f), new Vector2(190f, 48f));

        Button saveButton = CreateButton("SaveButton", rowRect, "Сохранить");
        SetLocalRect(saveButton.GetComponent<RectTransform>(), new Vector2(350f, 0f), new Vector2(130f, 42f));
        saveButton.GetComponentInChildren<Text>().fontSize = 18;

        CheckpointSaveSlotView rowView = rowObject.AddComponent<CheckpointSaveSlotView>();
        rowView.SetReferences(titleText, detailText, timestampText, saveButton);
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

    private static void CreateColumnHeader(
        string name,
        Transform parent,
        string text,
        Vector2 position,
        Vector2 size)
    {
        RectTransform headerRect = CreateLabel(name, parent, text, 16, TextAnchor.MiddleCenter);
        SetTopRect(headerRect, new Vector2(0.5f, 1f), size);
        headerRect.anchoredPosition = position;
        Text header = headerRect.GetComponent<Text>();
        header.text = text;
        header.color = new Color(0.72f, 0.72f, 0.72f, 1f);
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
