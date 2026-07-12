using System;
using ChainSawLeg.Core.SaveSystem;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationSaveContributor : IGameSaveContributor
    {
        public const string Id = "exploration";

        private readonly IPlayerPositionProvider playerPositionProvider;
        private readonly IExplorationSaveContextProvider saveContextProvider;

        public ExplorationSaveContributor(
            IPlayerPositionProvider playerPositionProvider,
            IExplorationSaveContextProvider saveContextProvider)
        {
            this.playerPositionProvider = playerPositionProvider;
            this.saveContextProvider = saveContextProvider;
        }

        public string ContributorId => Id;
        public Type SaveDataType => typeof(ExplorationSaveData);

        public object CaptureSaveData()
        {
            if (!playerPositionProvider.IsPlayerAvailable)
                throw new GameSaveValidationException("Player is not available for exploration save capture.");

            return new ExplorationSaveData
            {
                SceneId = saveContextProvider.SceneId,
                CheckpointId = saveContextProvider.CheckpointId,
                PositionX = playerPositionProvider.PositionX,
                PositionY = playerPositionProvider.PositionY
            };
        }
    }
}
