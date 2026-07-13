using System;

public sealed class SaveSlotViewData
{
    public SaveSlotViewData(
        string kindLabel,
        string slotId,
        string checkpointId,
        string sceneName,
        DateTime utcTimestamp,
        bool isLoadable,
        bool isEmpty)
    {
        KindLabel = kindLabel;
        SlotId = slotId;
        CheckpointId = checkpointId;
        SceneName = sceneName;
        UtcTimestamp = utcTimestamp;
        IsLoadable = isLoadable;
        IsEmpty = isEmpty;
    }

    public string KindLabel { get; }
    public string SlotId { get; }
    public string CheckpointId { get; }
    public string SceneName { get; }
    public DateTime UtcTimestamp { get; }
    public bool IsLoadable { get; }
    public bool IsEmpty { get; }
}
