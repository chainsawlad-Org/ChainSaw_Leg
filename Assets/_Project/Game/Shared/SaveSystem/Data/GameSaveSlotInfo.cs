using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveSlotInfo
    {
        public GameSaveSlotInfo(
            string slotId,
            GameSaveMetadata metadata,
            bool isCorrupted)
        {
            SlotId = slotId;
            Metadata = metadata;
            IsCorrupted = isCorrupted;
        }

        public string SlotId { get; }
        public GameSaveMetadata Metadata { get; }
        public bool IsCorrupted { get; }
    }
}
