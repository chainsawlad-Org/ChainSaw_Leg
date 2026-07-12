using System;
using System.Collections.Generic;
using ChainSawLeg.Core.SaveSystem;
using UnityEngine;
using UnityEngine.UI;

public sealed class SaveBrowserView : MonoBehaviour
{
    [SerializeField] private RectTransform rowsContainer;
    [SerializeField] private SaveSlotView rowTemplate;
    [SerializeField] private Text statusText;
    [SerializeField] private Text errorText;
    [SerializeField] private ScrollRect scrollRect;

    private readonly List<SaveSlotView> rows = new();
    private bool interactionEnabled = true;

    public event Action<GameSaveCatalogEntry> LoadRequested;

    public void SetReferences(
        RectTransform rowsContainer,
        SaveSlotView rowTemplate,
        Text statusText,
        Text errorText,
        ScrollRect scrollRect)
    {
        this.rowsContainer = rowsContainer;
        this.rowTemplate = rowTemplate;
        this.statusText = statusText;
        this.errorText = errorText;
        this.scrollRect = scrollRect;
        rowTemplate.gameObject.SetActive(false);
    }

    public void ShowLoading()
    {
        HideRows();
        errorText.gameObject.SetActive(false);
        statusText.text = "Загрузка сохранений...";
        statusText.gameObject.SetActive(true);
        ResetScrollPosition();
    }

    public void ShowEntries(IReadOnlyList<GameSaveCatalogEntry> entries)
    {
        EnsureRowCount(entries.Count);

        for (int index = 0; index < rows.Count; index++)
        {
            if (index < entries.Count)
                rows[index].Show(entries[index], interactionEnabled);
            else
                rows[index].Hide();
        }

        statusText.text = entries.Count == 0 ? "Сохранений нет" : string.Empty;
        statusText.gameObject.SetActive(entries.Count == 0);
        errorText.gameObject.SetActive(false);
    }

    public void ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        statusText.gameObject.SetActive(false);
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        interactionEnabled = isEnabled;

        foreach (SaveSlotView row in rows)
            row.SetInteractionEnabled(isEnabled);
    }

    public void AppendInteractableButtons(List<Button> buttons)
    {
        foreach (SaveSlotView row in rows)
        {
            if (row.gameObject.activeSelf && row.LoadButton.IsInteractable())
                buttons.Add(row.LoadButton);
        }
    }

    public Button GetFirstInteractableButton()
    {
        foreach (SaveSlotView row in rows)
        {
            if (row.gameObject.activeSelf && row.LoadButton.IsInteractable())
                return row.LoadButton;
        }

        return null;
    }

    public void EnsureButtonVisible(Button button)
    {
        if (button == null || scrollRect == null || scrollRect.viewport == null)
            return;

        Canvas.ForceUpdateCanvases();

        RectTransform buttonRect = button.GetComponent<RectTransform>();
        Bounds buttonBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            scrollRect.viewport,
            buttonRect);
        Rect viewportRect = scrollRect.viewport.rect;
        Vector2 contentPosition = scrollRect.content.anchoredPosition;

        if (buttonBounds.max.y > viewportRect.yMax)
            contentPosition.y += viewportRect.yMax - buttonBounds.max.y;
        else if (buttonBounds.min.y < viewportRect.yMin)
            contentPosition.y += viewportRect.yMin - buttonBounds.min.y;

        scrollRect.StopMovement();
        scrollRect.content.anchoredPosition = contentPosition;
    }

    private void EnsureRowCount(int count)
    {
        while (rows.Count < count)
        {
            SaveSlotView row = Instantiate(rowTemplate, rowsContainer);
            row.name = $"SaveSlot_{rows.Count}";
            row.LoadClicked += HandleLoadClicked;
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchoredPosition = new Vector2(0f, -rows.Count * 64f);
            rows.Add(row);
        }

        float contentHeight = rows.Count * 64f;

        if (rowsContainer.sizeDelta.y < contentHeight)
            rowsContainer.sizeDelta = new Vector2(rowsContainer.sizeDelta.x, contentHeight);
    }

    private void HideRows()
    {
        foreach (SaveSlotView row in rows)
            row.Hide();
    }

    private void HandleLoadClicked(GameSaveCatalogEntry entry)
    {
        LoadRequested?.Invoke(entry);
    }

    private void ResetScrollPosition()
    {
        if (scrollRect == null)
            return;

        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
