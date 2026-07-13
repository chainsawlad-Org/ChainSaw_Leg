using System;

namespace ChainSawLeg.Features.Exploration.Save
{
    [Serializable]
    public sealed class ExplorationSaveData
    {
        private ExplorationSaveData()
        {
        }

        public ExplorationSaveData(
            string sceneId,
            string checkpointId,
            float positionX,
            float positionY)
        {
            SceneId = sceneId;
            CheckpointId = checkpointId;
            PositionX = positionX;
            PositionY = positionY;
        }

        public string SceneId { get; private set; }
        public string CheckpointId { get; private set; }
        public float PositionX { get; private set; }
        public float PositionY { get; private set; }
    }
}
