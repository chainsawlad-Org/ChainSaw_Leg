using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CheckpointSaveMenuView : MonoBehaviour
{
    private CheckpointSaveSlotView[] rows;
    private Button backButton;
    private GameObject root;
    private ScrollRect scrollRect;
    private bool interactionEnabled = true;

    public event Action<string> SlotSaveRequested;
    public event Action BackRequested;

    public void SetReferences(
        CheckpointSaveSlotView[] rows,
        Button backButton,
        GameObject root,
        ScrollRect scrollRect)
    {
        this.rows = rows;
        this.backButton = backButton;
        this.root = root;
        this.scrollRect = scrollRect;

        foreach (CheckpointSaveSlotView row in rows)
            row.SaveClicked += HandleSlotSaveClicked;

        backButton.onClick.AddListener(HandleBackClicked);
        root.SetActive(false);
    }

    public void ShowEntries(IReadOnlyList<SaveSlotViewData> entries)
    {
        for (int index = 0; index < rows.Length; index++)
        {
            if (index < entries.Count)
                rows[index].Show(entries[index], interactionEnabled);
            else
                rows[index].Hide();
        }
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        interactionEnabled = isEnabled;
        backButton.interactable = isEnabled;

        foreach (CheckpointSaveSlotView row in rows)
            row.SetInteractionEnabled(isEnabled);
    }

    public void Show()
    {
        root.SetActive(true);
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 1f;
        SelectFirstInteractableRow();
    }

    public void Hide()
    {
        root.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    private void SelectFirstInteractableRow()
    {
        if (EventSystem.current == null)
            return;

        foreach (CheckpointSaveSlotView row in rows)
        {
            if (!row.gameObject.activeSelf || !row.SaveButton.IsInteractable())
                continue;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(row.SaveButton.gameObject);
            row.SaveButton.Select();
            return;
        }
    }

    private void HandleSlotSaveClicked(string slotId)
    {
        SlotSaveRequested?.Invoke(slotId);
    }

    private void HandleBackClicked()
    {
        BackRequested?.Invoke();
    }
}
