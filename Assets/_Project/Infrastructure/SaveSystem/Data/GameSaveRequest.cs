// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveRequest
    {
        public GameSaveKind Kind;
        public string SlotId;
        public string CheckpointId;

        public GameSaveRequest()
        {
        }

        public GameSaveRequest(GameSaveKind kind, string slotId, string checkpointId = null)
        {
            Kind = kind;
            SlotId = slotId;
            CheckpointId = checkpointId;
        }
    }
}
