using UnityEngine;
using UnityEngine.UI;

public static class RuntimeUIElementFactory
{
    public static Scrollbar CreateVerticalScrollbar(
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

    public static Text CreateRowText(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size)
    {
        RectTransform labelRect = CreateLabel(name, parent, string.Empty, 18, TextAnchor.MiddleCenter);
        SetLocalRect(labelRect, position, size);
        return labelRect.GetComponent<Text>();
    }

    public static void CreateColumnHeader(
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

    public static RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        Stretch(rectTransform);

        Image image = panelObject.GetComponent<Image>();
        image.color = color;

        return rectTransform;
    }

    public static Button CreateButton(string name, Transform parent, string label)
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

    public static RectTransform CreateLabel(string name, Transform parent, string value, int fontSize, TextAnchor anchor)
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

    public static Font GetBuiltInFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    public static void SetPanelRect(RectTransform rectTransform, Vector2 anchor, Vector2 size)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }

    public static void SetTopRect(RectTransform rectTransform, Vector2 anchor, Vector2 size)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }

    public static void SetLocalRect(RectTransform rectTransform, Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }
}
