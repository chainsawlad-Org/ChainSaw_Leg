using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveRequest
    {
        public GameSaveRequest(GameSaveKind kind, string slotId, string checkpointId = null)
        {
            Kind = kind;
            SlotId = slotId;
            CheckpointId = checkpointId;
        }

        public GameSaveKind Kind { get; }
        public string SlotId { get; }
        public string CheckpointId { get; }
    }
}
