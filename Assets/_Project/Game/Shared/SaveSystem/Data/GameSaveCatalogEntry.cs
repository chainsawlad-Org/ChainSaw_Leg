using System;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class GameSaveCatalogEntry
    {
        public GameSaveCatalogEntry(
            GameSaveKind kind,
            string slotId,
            string checkpointId,
            string sceneId,
            string sceneName,
            DateTime utcTimestamp,
            bool isLoadable,
            bool isEmpty = false)
        {
            Kind = kind;
            SlotId = slotId;
            CheckpointId = checkpointId;
            SceneId = sceneId;
            SceneName = sceneName;
            UtcTimestamp = utcTimestamp;
            IsLoadable = isLoadable;
            IsEmpty = isEmpty;
        }

        public GameSaveKind Kind { get; }
        public string SlotId { get; }
        public string CheckpointId { get; }
        public string SceneId { get; }
        public string SceneName { get; }
        public DateTime UtcTimestamp { get; }
        public bool IsLoadable { get; }
        public bool IsEmpty { get; }
    }
}
