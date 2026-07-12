using System;
using ChainSawLeg.Core.SaveSystem;
using UnityEngine;
using UnityEngine.UI;

public sealed class SaveSlotView : MonoBehaviour
{
    [SerializeField] private Text kindText;
    [SerializeField] private Text locationText;
    [SerializeField] private Text timestampText;
    [SerializeField] private Text sceneText;
    [SerializeField] private Button loadButton;

    private GameSaveCatalogEntry entry;

    public event Action<GameSaveCatalogEntry> LoadClicked;

    public Button LoadButton => loadButton;

    public void SetReferences(
        Text kindText,
        Text locationText,
        Text timestampText,
        Text sceneText,
        Button loadButton)
    {
        this.kindText = kindText;
        this.locationText = locationText;
        this.timestampText = timestampText;
        this.sceneText = sceneText;
        this.loadButton = loadButton;
    }

    private void OnEnable()
    {
        loadButton.onClick.AddListener(HandleLoadClicked);
    }

    private void OnDisable()
    {
        loadButton.onClick.RemoveListener(HandleLoadClicked);
    }

    public void Show(GameSaveCatalogEntry catalogEntry, bool interactionEnabled)
    {
        entry = catalogEntry;
        kindText.text = catalogEntry.Kind.ToString();
        locationText.text = string.IsNullOrWhiteSpace(catalogEntry.CheckpointId)
            ? catalogEntry.SlotId
            : catalogEntry.CheckpointId;
        timestampText.text = catalogEntry.UtcTimestamp == DateTime.MinValue
            ? "—"
            : catalogEntry.UtcTimestamp.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        sceneText.text = string.IsNullOrWhiteSpace(catalogEntry.SceneName)
            ? "—"
            : catalogEntry.SceneName;
        loadButton.interactable = interactionEnabled && catalogEntry.IsLoadable;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        entry = null;
        gameObject.SetActive(false);
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        loadButton.interactable = isEnabled && entry != null && entry.IsLoadable;
    }

    private void HandleLoadClicked()
    {
        if (entry != null && entry.IsLoadable)
            LoadClicked?.Invoke(entry);
    }
}
