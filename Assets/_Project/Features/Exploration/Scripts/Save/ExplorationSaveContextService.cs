using ChainSawLeg.Core.SaveSystem;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationSaveContextService : IExplorationSaveContextProvider
    {
        public string SceneId { get; private set; } = ExplorationSceneIds.World;
        public string CheckpointId { get; private set; }

        public void SetContext(string sceneId, string checkpointId)
        {
            if (string.IsNullOrWhiteSpace(sceneId))
                throw new GameSaveValidationException("Exploration scene ID is required.");

            SceneId = sceneId;
            CheckpointId = checkpointId;
        }
    }
}
