using UnityEngine;
using UnityEngine.UI;
using static RuntimeUIElementFactory;

public static class CheckpointSaveMenuViewFactory
{
    public static CheckpointSaveMenuView BuildCheckpointSaveMenuView(
        Transform parent,
        int slotCount)
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
        rowsContainer.sizeDelta = new Vector2(0f, slotCount * 64f);

        var rows = new CheckpointSaveSlotView[slotCount];

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
}
