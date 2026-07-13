using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CheckpointSaveSlotView : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text detailText;
    [SerializeField] private Text timestampText;
    [SerializeField] private Button saveButton;

    private SaveSlotViewData entry;

    public event Action<string> SaveClicked;

    public Button SaveButton => saveButton;

    public void SetReferences(
        Text titleText,
        Text detailText,
        Text timestampText,
        Button saveButton)
    {
        this.titleText = titleText;
        this.detailText = detailText;
        this.timestampText = timestampText;
        this.saveButton = saveButton;
    }

    private void OnEnable()
    {
        saveButton.onClick.AddListener(HandleSaveClicked);
    }

    private void OnDisable()
    {
        saveButton.onClick.RemoveListener(HandleSaveClicked);
    }

    public void Show(SaveSlotViewData catalogEntry, bool interactionEnabled)
    {
        entry = catalogEntry;
        titleText.text = BuildSlotLabel(catalogEntry.SlotId);
        detailText.text = catalogEntry.IsEmpty
            ? "Пусто"
            : string.IsNullOrWhiteSpace(catalogEntry.CheckpointId)
                ? "—"
                : catalogEntry.CheckpointId;
        timestampText.text = catalogEntry.IsEmpty || catalogEntry.UtcTimestamp == DateTime.MinValue
            ? "—"
            : catalogEntry.UtcTimestamp.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        saveButton.interactable = interactionEnabled;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        entry = null;
        gameObject.SetActive(false);
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        saveButton.interactable = isEnabled;
    }

    private static string BuildSlotLabel(string slotId)
    {
        int separatorIndex = slotId.LastIndexOf('_');

        if (separatorIndex >= 0 && int.TryParse(slotId.Substring(separatorIndex + 1), out int index))
            return $"Слот {index + 1}";

        return slotId;
    }

    private void HandleSaveClicked()
    {
        if (entry != null)
            SaveClicked?.Invoke(entry.SlotId);
    }
}
