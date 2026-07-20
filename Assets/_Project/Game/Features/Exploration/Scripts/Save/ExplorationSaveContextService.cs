using ChainSawLeg.Core.SaveSystem;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class ExplorationSaveContextService : IExplorationSaveContextProvider
    {
        private readonly ISceneLoader sceneLoader;
        private string sceneId = ExplorationSceneIds.World;

        public ExplorationSaveContextService(ISceneLoader sceneLoader = null)
        {
            this.sceneLoader = sceneLoader;
        }

        public string SceneId
        {
            get
            {
                string loadedScene = sceneLoader?.LoadedGameplayScene;

                if (loadedScene == SceneNames.World ||
                    loadedScene == SceneNames.WorldOld)
                    return loadedScene;

                return sceneId;
            }
        }

        public string CheckpointId { get; private set; }

        public void SetContext(string sceneId, string checkpointId)
        {
            if (string.IsNullOrWhiteSpace(sceneId))
                throw new GameSaveValidationException("Exploration scene ID is required.");

            this.sceneId = sceneId;
            CheckpointId = checkpointId;
        }
    }
}
