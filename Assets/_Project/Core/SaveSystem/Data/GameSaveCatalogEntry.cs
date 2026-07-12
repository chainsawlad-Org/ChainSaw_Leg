using System;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class GameSaveCatalogEntry
    {
        public GameSaveKind Kind { get; set; }
        public string SlotId { get; set; }
        public string CheckpointId { get; set; }
        public string SceneId { get; set; }
        public string SceneName { get; set; }
        public DateTime UtcTimestamp { get; set; }
        public bool IsLoadable { get; set; }
    }
}
