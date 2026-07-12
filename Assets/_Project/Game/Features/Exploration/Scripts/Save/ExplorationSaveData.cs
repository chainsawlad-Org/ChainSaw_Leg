using System;

namespace ChainSawLeg.Features.Exploration.Save
{
    [Serializable]
    public sealed class ExplorationSaveData
    {
        public string SceneId;
        public string CheckpointId;
        public float PositionX;
        public float PositionY;
    }
}
