using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveSlotInfo
    {
        public string SlotId;
        public GameSaveMetadata Metadata;
        public bool IsCorrupted;
    }
}
