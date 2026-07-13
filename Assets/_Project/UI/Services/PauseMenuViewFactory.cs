using UnityEngine;
using UnityEngine.UI;
using static RuntimeUIElementFactory;

public static class PauseMenuViewFactory
{
    public static PauseMenuView BuildPauseMenuView(
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
}
