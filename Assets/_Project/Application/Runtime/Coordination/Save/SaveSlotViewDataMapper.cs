using System.Collections.Generic;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.UI;

public static class SaveSlotViewDataMapper
{
    public static IReadOnlyList<SaveSlotViewData> Map(
        IReadOnlyList<GameSaveCatalogEntry> entries)
    {
        var result = new List<SaveSlotViewData>(entries.Count);

        foreach (GameSaveCatalogEntry entry in entries)
        {
            result.Add(new SaveSlotViewData(
                entry.Kind.ToString(),
                entry.SlotId,
                entry.CheckpointId,
                entry.SceneName,
                entry.UtcTimestamp,
                entry.IsLoadable,
                entry.IsEmpty));
        }

        return result;
    }
}
