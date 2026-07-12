// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

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
