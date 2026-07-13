using System;
using ChainSawLeg.Core.SaveSystem;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationSaveRestorer : IGameSaveRestorer
    {
        private readonly IPlayerPositionRestorationTarget restorationTarget;
        private readonly ExplorationSaveContextService saveContextService;

        public ExplorationSaveRestorer(
            IPlayerPositionRestorationTarget restorationTarget,
            ExplorationSaveContextService saveContextService)
        {
            this.restorationTarget = restorationTarget;
            this.saveContextService = saveContextService;
        }

        public string ContributorId => ExplorationSaveContributor.Id;
        public Type SaveDataType => typeof(ExplorationSaveData);

        public void RestoreSaveData(object saveData)
        {
            if (saveData is not ExplorationSaveData explorationSaveData)
                throw new GameSaveValidationException("Exploration save DTO has an unexpected type.");

            ValidatePosition(explorationSaveData.PositionX, explorationSaveData.PositionY);
            saveContextService.SetContext(
                explorationSaveData.SceneId,
                explorationSaveData.CheckpointId);
            restorationTarget.RestorePosition(
                explorationSaveData.PositionX,
                explorationSaveData.PositionY);
        }

        private static void ValidatePosition(float positionX, float positionY)
        {
            if (float.IsNaN(positionX) || float.IsInfinity(positionX) ||
                float.IsNaN(positionY) || float.IsInfinity(positionY))
                throw new GameSaveValidationException("Exploration player position is invalid.");
        }
    }
}
